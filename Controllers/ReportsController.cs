
using ExpenseTracker.API.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ExpenseTracker.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        // Common user validation
        private bool TryGetUserId(out int userId)
        {
            var claim = User.FindFirst(
                ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(claim, out userId);
        }

        // Common date validation
        private static bool ValidDates(DateOnly fromDate, DateOnly toDate)
        {
            return fromDate <= toDate;
        }

        // Get individual income and expense records
        [HttpGet("transactions")]
        public async Task<IActionResult> GetTransactions(
            [FromQuery] DateOnly fromDate,
            [FromQuery] DateOnly toDate)
        {
            if (!ValidDates(fromDate, toDate))
                return BadRequest("From date cannot be after to date.");

            if (!TryGetUserId(out int userId))
                return Unauthorized("Invalid user.");

            var endDate = toDate.AddDays(1);

            var incomes = await _context.incomes
                .AsNoTracking()
                .Where(i =>
                    i.user_id == userId &&
                    i.income_date >= fromDate &&
                    i.income_date < endDate)
                .OrderByDescending(i => i.income_date)
                .Select(i => new
                {
                    id = i.id,
                    title = i.title,
                    amount = i.amount,
                    date = i.income_date,
                    notes = i.notes
                })
                .ToListAsync();

            var expenses = await _context.expenses
                .AsNoTracking()
                .Where(e =>
                    e.user_id == userId &&
                    e.expense_date >= fromDate &&
                    e.expense_date < endDate)
                .OrderByDescending(e => e.expense_date)
                .Select(e => new
                {
                    id = e.id,
                    title = e.title,
                    category = e.category,
                    amount = e.amount,
                    date = e.expense_date,
                    notes = e.notes
                })
                .ToListAsync();

            return Ok(new
            {
                incomes,
                expenses
            });
        }

       

      
    }
}
