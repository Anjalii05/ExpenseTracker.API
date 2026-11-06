namespace ExpenseTracker.API.Models
{
    

    public class User
    {
        public int id { get; set; }

        public string full_name { get; set; } = string.Empty;

        public string email { get; set; } = string.Empty;

        public string password { get; set; } = string.Empty;

        public DateTime created_at { get; set; } = DateTime.UtcNow;

        public string? phone_number { get; set; }

        public string? profile_image { get; set; }

        public string? reset_password_token { get; set; }
        public DateTime? reset_password_token_expiry { get; set; }

        public ICollection<Expense> expenses { get; set; } = new List<Expense>();
    }
}
