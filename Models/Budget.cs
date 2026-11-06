
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ExpenseTracker.API.Models
{
    public class Budget
    {
        [Key]
        public int id { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        public decimal amount { get; set; }

        [Required]
        public string month { get; set; } = string.Empty;

        [Required]
        public int year { get; set; }

        public string? notes { get; set; }

        [Required]
        public int user_id { get; set; }

        public DateTime created_at { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(user_id))]
        public User user { get; set; } = null!;
    }
}
