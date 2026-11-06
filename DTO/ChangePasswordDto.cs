namespace ExpenseTracker.API.DTO
{
    public class ChangePasswordDto
    {
        public string currentPassword { get; set; } = string.Empty;
        public string newPassword { get; set; } = string.Empty;
        public string confirmPassword { get; set; } = string.Empty;
    }
}
