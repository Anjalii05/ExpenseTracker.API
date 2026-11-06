using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.API.DTO
{
    public class LoginDto
    {
        [Required]
        public string email { get; set; } = string.Empty;

        [Required]
        public string password { get; set; } = string.Empty;
    }
}