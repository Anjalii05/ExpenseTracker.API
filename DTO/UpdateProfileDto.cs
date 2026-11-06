namespace ExpenseTracker.API.DTO
{
    public class UpdateProfileDto
    {
        public string fullName { get; set; } = string.Empty;
        public string email { get; set; } = string.Empty;
        public string? phoneNumber { get; set; }
    }
}
