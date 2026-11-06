using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.API.DTO
{
    public class IncomeDto
    {
        [Required]
        public string title { get; set; }

        [Required]
        public decimal amount { get; set; }

        [Required]
        public DateOnly incomeDate { get; set; }

        public string? notes { get; set; }
    }
}
