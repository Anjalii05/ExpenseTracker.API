using ExpenseTracker.API.Data;
using ExpenseTracker.API.DTO;
using ExpenseTracker.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ExpenseTracker.API.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class BudgetController : ControllerBase
    {
        private readonly AppDbContext _context;
        public BudgetController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("setBudget")]
        public async Task<IActionResult> SetBudget([FromBody] BudgetDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            // Get the logged-in user's ID from the JWT claims.
            var userIdClaim = User.FindFirst(
                System.Security.Claims.ClaimTypes.NameIdentifier
            )?.Value;

            if (!int.TryParse(userIdClaim, out int userId))
                return Unauthorized("Invalid user.");

            // Check whether a budget already exists for this user,
            // month and year.
            var existingBudget = await _context.budgets
                .FirstOrDefaultAsync(b =>
                    b.user_id == userId &&
                    b.month == dto.month &&
                    b.year == dto.year
                );

            if (existingBudget != null)
            {
                // Update the existing budget.
                existingBudget.amount = dto.amount;
                existingBudget.notes = dto.notes;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    message = "Budget updated successfully.",
                    budget = existingBudget
                });
            }

            // Insert a new budget if none exists.
            var newBudget = new Budget
            {
                amount = dto.amount,
                month = dto.month,
                year = dto.year,
                notes = dto.notes,
                user_id = userId,
                created_at = DateTime.UtcNow
            };

            _context.budgets.Add(newBudget);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Budget added successfully.",
                budget = newBudget
            });
        }







        [HttpGet("dashboard")]
        public async Task<IActionResult> GetDashboard([FromQuery] int month,[FromQuery] int year)
        {
            if (month < 1 || month > 12 || year < 2000) return BadRequest("Invalid month or year.");
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (!int.TryParse(userIdClaim, out int userId)) return Unauthorized("Invalid user.");
            // Convert month number to month name
            //var monthName = System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month);
            // Get budget for the selected month
            var monthName = System.Globalization.CultureInfo.InvariantCulture.DateTimeFormat.GetMonthName(month);
            var budget = await _context.budgets.Where(b =>b.user_id == userId && b.month == monthName && b.year == year).Select(b => (decimal?)b.amount).FirstOrDefaultAsync() ?? 0m;

            // Get total expenses for the selected month
            var startDate = new DateOnly(year, month, 1);
            var endDate = startDate.AddMonths(1);

            var amountSpent = await _context.expenses.Where(e =>
                    e.user_id == userId &&
                    e.expense_date >= startDate &&
                    e.expense_date < endDate)
                .SumAsync(e => (decimal?)e.amount) ?? 0m;

            // Calculate remaining amount
            var remaining = budget - amountSpent;

            // Calculate progress percentage
            var progressPercentage = budget > 0? Math.Round((amountSpent / budget) * 100, 2): 0m;

            // Calculate budget health
            string budgetHealth;

            if (budget <= 0)
                budgetHealth = "No budget set";
            else if (amountSpent >= budget)
                budgetHealth = "Over budget";
            else if (progressPercentage >= 80)
                budgetHealth = "Near budget limit";
            else
                budgetHealth = "On track";

            return Ok(new
            {
                monthlyBudget = budget,
                amountSpent = amountSpent,
                remaining = remaining,
                progressPercentage = progressPercentage,
                budgetHealth = budgetHealth,
                month = month,
                year = year
            });
        }

    }
}
