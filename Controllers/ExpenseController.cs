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
    public class ExpenseController : ControllerBase
    {
        private readonly AppDbContext _context;
        public ExpenseController(AppDbContext context)
        {
            _context = context;
        }
        [HttpPost("addExpense")]
        public async Task<IActionResult> CreateExpenseAsync(ExpenseDto expenseDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new
                {
                    message = "User not authenticated."
                });
            }

            int userId = Convert.ToInt32(userIdClaim);

            var expense = new Expense
            {
                title = expenseDto.title,
                amount = expenseDto.amount,
                notes = expenseDto.notes,
                category = expenseDto.category,
                expense_date = expenseDto.expenseDate,
                user_id = userId,
                created_at = DateTime.UtcNow
            };

            _context.expenses.Add(expense);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Expense Added Successfully"
            });
        }


        //[HttpGet("getExpense")]
        //public async Task<IActionResult> GetExpenses()
        //{
        //    var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        //    if (string.IsNullOrEmpty(userIdClaim))
        //    {
        //        return Unauthorized(new
        //        {
        //            message = "User not authenticated."
        //        });
        //    }

        //    int userId = Convert.ToInt32(userIdClaim);

        //    var expenses = await _context.expenses.Where(x => x.user_id == userId).OrderByDescending(x => x.expense_date).ToListAsync();

        //    return Ok(new
        //    {
        //        message = "Expenses fetched successfully",
        //        data = expenses
        //    });
        //}

        


        [HttpPut("updateExpense/{id}")]
        public async Task<IActionResult> UpdateExpense(int id, ExpenseDto expenseDto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new
                {
                    message = "User not authenticated."
                });
            }
            int userId = Convert.ToInt32(userIdClaim);
            var expense = await _context.expenses.FirstOrDefaultAsync(x =>x.id == id &&x.user_id == userId);
            if (expense == null)
            {
                return NotFound(new
                {
                    message = "Expense not found."
                });
            }
            expense.title = expenseDto.title;
            expense.amount = expenseDto.amount;
            expense.category = expenseDto.category;
            expense.expense_date = expenseDto.expenseDate;
            expense.notes = expenseDto.notes;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Expense Updated Successfully"
            });
        }

        [HttpDelete("deleteExpense/{id}")]
        public async Task<IActionResult> DeleteExpense(int id)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new
                {
                    message = "User not authenticated."
                });
            }
            int userId = Convert.ToInt32(userIdClaim);
            var expense = await _context.expenses.FirstOrDefaultAsync(x => x.id == id && x.user_id == userId);
            if (expense == null)
            {
                return NotFound(new
                {
                    message = "Expense not found."
                });
            }
            _context.expenses.Remove(expense);
            await _context.SaveChangesAsync();
            return Ok(new
            {
                message = "Expense Deleted Successfully"
            });


        }

        [HttpGet("getExpenses")]
        public async Task<IActionResult> GetExpenses(string? search,int? month)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(new
                {
                    message = "User not authenticated."
                });
            }
            int userId = Convert.ToInt32(userIdClaim);
            var query = _context.expenses.Where(x => x.user_id == userId).AsQueryable();
            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>x.title.ToLower().Contains(search.ToLower()));
            }
            //if (!string.IsNullOrWhiteSpace(category))
            //{
            //    query = query.Where(x =>x.category == category);
            //}
            if (month.HasValue)
            {
                query = query.Where(x =>x.expense_date.Month == month.Value);
            }
            var expenses = await query.OrderByDescending(x => x.expense_date).ToListAsync();
            return Ok(new
            {
                message = "Expenses fetched successfully",
                data = expenses
            });
        }




    }
}
