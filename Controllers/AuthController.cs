using ExpenseTracker.API.Controllers;
using ExpenseTracker.API.Data;
using ExpenseTracker.API.DTO;
using ExpenseTracker.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using System.Text;
using ExpenseTracker.API.Services;




namespace ExpenseTracker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly EmailService _emailService;
        public AuthController(AppDbContext context, IConfiguration configuration, EmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }


        private string GenerateJwtToken(User user)
        {
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.id.ToString()),
                new Claim(ClaimTypes.Name, user.full_name),
                new Claim(ClaimTypes.Email, user.email)
            };

            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)
            );

            var credentials = new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256
            );

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(_configuration["Jwt:DurationInMinutes"])
                ),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            
            if (string.IsNullOrWhiteSpace(registerDto.full_name) ||string.IsNullOrWhiteSpace(registerDto.email) ||string.IsNullOrWhiteSpace(registerDto.password))
            {
                return BadRequest(new { message = "All fields are required." });
            }
            var existingUser = await _context.users.FirstOrDefaultAsync(x => x.email == registerDto.email);

            if (existingUser != null)
            {
                return BadRequest(new { message = "Email already exist" });
            }
            if (registerDto.password != registerDto.confirmPassword)
            {
                return BadRequest(new
                {
                    message = "Password and Confirm Password do not match."
                });
            }
            var hashedPassword = BCrypt.Net.BCrypt.HashPassword(registerDto.password);

            var user = new User
            {
                full_name = registerDto.full_name,
                email = registerDto.email,
                password = hashedPassword,
                created_at = DateTime.UtcNow
            };

            _context.users.Add(user);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Registration Successful"
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (string.IsNullOrWhiteSpace(loginDto.email) || string.IsNullOrWhiteSpace(loginDto.password))
            {
                return BadRequest(new { message = "Email and Password are required." });
            }
            var user = await _context.users.FirstOrDefaultAsync(x => x.email == loginDto.email);

            if (user == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid Email or Password."
                });
            }
            

            

            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(
                loginDto.password,
                user.password
            );

            if (!isPasswordValid)
            {
                return Unauthorized(new
                {
                    message = "Invalid Email or Password."
                });
            }
            var token = GenerateJwtToken(user);
            return Ok(new
            {
                message = "Login Successful",
                token = token,
                user = new
                {
                    id = user.id,
                    fullName = user.full_name,
                    email = user.email
                }
            });
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user." });
            }

            var user = await _context.users.Where(u => u.id == userId).Select(u => new
                {
                    id = u.id,
                    fullName = u.full_name,
                    email = u.email,
                    phoneNumber = u.phone_number,
                    profileImage = u.profile_image
                })
                .FirstOrDefaultAsync();

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(new
            {
                message = "Profile fetched successfully",
                data = user
            });
        }

        [HttpPut("updateProfile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile(UpdateProfileDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user." });
            }

            if (string.IsNullOrWhiteSpace(dto.fullName) ||
                string.IsNullOrWhiteSpace(dto.email))
            {
                return BadRequest(new { message = "Name and email are required." });
            }

            var user = await _context.users.FindAsync(userId);

            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var emailExists = await _context.users.AnyAsync(
                u => u.email == dto.email && u.id != userId
            );

            if (emailExists)
            {
                return BadRequest(new { message = "Email is already registered." });
            }

            user.full_name = dto.fullName.Trim();
            user.email = dto.email.Trim();
            user.phone_number = dto.phoneNumber?.Trim();

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Profile updated successfully."
            });
        }


        [HttpPut("changePassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
            {
                return Unauthorized(new { message = "Invalid user." });
            }
            if (string.IsNullOrWhiteSpace(dto.currentPassword) ||
                string.IsNullOrWhiteSpace(dto.newPassword) ||
                string.IsNullOrWhiteSpace(dto.confirmPassword))
            {
                return BadRequest(new { message = "All password fields are required." });
            }
            if (dto.newPassword != dto.confirmPassword)
            {
                return BadRequest(new { message = "New password and confirm password do not match." });
            }
            if (dto.newPassword.Length < 8)
            {
                return BadRequest(new { message = "New password must contain at least 8 characters." });
            }
            if (dto.currentPassword == dto.newPassword)
            {
                return BadRequest(new { message = "New password must be different from current password." });
            }
            var user = await _context.users.FindAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(
                dto.currentPassword,
                user.password
            );

            if (!isPasswordValid)
            {
                return BadRequest(new { message = "Current password is incorrect." });
            }
            user.password = BCrypt.Net.BCrypt.HashPassword(dto.newPassword);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Password changed successfully."
            });
        }


        [HttpPost("uploadProfileImage")]
        [Authorize]
        public async Task<IActionResult> UploadProfileImage(IFormFile image)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized(new { message = "Invalid user." });

            if (image == null || image.Length == 0)
                return BadRequest(new { message = "Please select an image." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Only JPG, PNG and WEBP images are allowed." });

            if (image.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "Image size cannot exceed 5 MB." });

            var user = await _context.users.FindAsync(userId);

            if (user == null)
                return NotFound(new { message = "User not found." });

            var uploadsFolder = Path.Combine(
                Directory.GetCurrentDirectory(),
                "wwwroot",
                "profile-images"
            );

            Directory.CreateDirectory(uploadsFolder);

            var fileName = $"{userId}_{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            user.profile_image = $"/profile-images/{fileName}";

            await _context.SaveChangesAsync();

            var imageUrl = $"{Request.Scheme}://{Request.Host}{user.profile_image}";

            return Ok(new
            {
                message = "Profile picture uploaded successfully.",
                profileImage = imageUrl
            });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.email))
            {
                return BadRequest(new { message = "Email is required." });
            }

            var user = await _context.users
                .FirstOrDefaultAsync(x => x.email == dto.email);

            if (user == null)
            {
                return Ok(new
                {
                    message = "If the email is registered, a reset link will be sent."
                });
            }

            // Generate secure random token
            var tokenBytes = RandomNumberGenerator.GetBytes(32);

            var token = Convert.ToBase64String(tokenBytes)
                .Replace("+", "-")
                .Replace("/", "_")
                .TrimEnd('=');

            user.reset_password_token = token;
            user.reset_password_token_expiry = DateTime.UtcNow.AddMinutes(15);

            await _context.SaveChangesAsync();

            // Create frontend reset link
            var resetLink =
                $"http://localhost:5173/reset-password?email={Uri.EscapeDataString(user.email)}&token={Uri.EscapeDataString(token)}";

            // Email body
            var emailBody = $@"
        <div style='font-family: Arial; padding: 20px;'>
            <h2>Password Reset Request</h2>
            <p>Hello {System.Net.WebUtility.HtmlEncode(user.full_name)},</p>
            <p>We received a request to reset your ExpenseTracker password.</p>
            <p>Click the button below to reset your password:</p>

            <a href='{System.Net.WebUtility.HtmlEncode(resetLink)}'
               style='background-color:#4F46E5;
                      color:white;
                      padding:12px 25px;
                      text-decoration:none;
                      border-radius:6px;
                      display:inline-block;'>
                Reset Password
            </a>

            <p>This link will expire in 15 minutes.</p>
            <p>If you did not request a password reset, please ignore this email.</p>
        </div>";

            // Send email
            await _emailService.SendEmailAsync(
                user.email,
                "ExpenseTracker - Password Reset",
                emailBody
            );

            return Ok(new
            {
                message = "If the email is registered, a reset link will be sent."
            });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.email) ||
                string.IsNullOrWhiteSpace(dto.token) ||
                string.IsNullOrWhiteSpace(dto.newPassword))
            {
                return BadRequest(new
                {
                    message = "Email, token and new password are required."
                });
            }

            var user = await _context.users
                .FirstOrDefaultAsync(x => x.email == dto.email);

            if (user == null)
            {
                return BadRequest(new { message = "Invalid reset request." });
            }

            if (user.reset_password_token != dto.token ||
                user.reset_password_token_expiry == null ||
                user.reset_password_token_expiry <= DateTime.UtcNow)
            {
                return BadRequest(new
                {
                    message = "Invalid or expired reset token."
                });
            }

            // Hash the new password using BCrypt
            user.password = BCrypt.Net.BCrypt.HashPassword(dto.newPassword);

            // Invalidate token after successful password reset
            user.reset_password_token = null;
            user.reset_password_token_expiry = null;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Password reset successfully. Please login with your new password."
            });
        }


    }
}
