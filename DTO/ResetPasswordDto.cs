namespace ExpenseTracker.API.DTO
{
    public class ResetPasswordDto
    {
        public string email { get; set; } = string.Empty;

        public string token { get; set; } = string.Empty;

        public string newPassword { get; set; } = string.Empty;
    }
}
