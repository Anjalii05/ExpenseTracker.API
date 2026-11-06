using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.API.Models
{
    public class Income
    {
        [Key]
        public int id { get; set; }

        [Required]
        public string title { get; set; } = string.Empty;

        [Required]
        public decimal amount { get; set; }

        [Required]
        public DateOnly income_date { get; set; }

        public string? notes { get; set; }

        public int user_id { get; set; }

        public DateTime created_at { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(user_id))]
        public User user { get; set; } = null!;
    }
}