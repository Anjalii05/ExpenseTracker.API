using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.API.DTO
{
    public class BudgetDto
    {
        [Required]
        public string month { get; set; }=string.Empty;

        [Required]
        public decimal amount { get; set; }

        [Required]
        public int year { get; set; }

        public string? notes { get; set; }
    }
}
