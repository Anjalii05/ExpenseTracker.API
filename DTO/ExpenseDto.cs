using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.API.DTO
{
    public class ExpenseDto
    {
        [Required]
        public string title { get; set; }

        [Required]
        public decimal amount { get; set; }

        [Required]
        public string category { get; set; }

        [Required]
        public DateOnly expenseDate { get; set; }

        public string? notes { get; set; }
        

    }
}
