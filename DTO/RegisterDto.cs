using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.API.DTO
{
    public class RegisterDto
    {
        [Required]
        public string full_name { get; set; } = string.Empty;

        [Required]
        public string email { get; set; } = string.Empty;

        [Required]
        public string password { get; set; } = string.Empty;

        [Required]
        public string confirmPassword { get; set; } = string.Empty;
    }
}