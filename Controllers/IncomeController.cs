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
    public class IncomeController : ControllerBase
    {
        private readonly AppDbContext _context;
        public IncomeController(AppDbContext context)
        {
            _context = context;
        }
        [HttpPost("addIncome")]
        public async Task<IActionResult> CreateIncomeAsync(IncomeDto incomeDto)
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

            var income = new Income
            {
                title = incomeDto.title,
                amount = incomeDto.amount,
                notes = incomeDto.notes,
                income_date = incomeDto.incomeDate,
                user_id = userId,
                created_at = DateTime.UtcNow
            };

            _context.incomes.Add(income);

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Income Added Successfully"
            });
        }


        //[HttpGet("getIncome")]
        //public async Task<IActionResult> GetIncomes()
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

        //    var incomes = await _context.incomes.Where(x => x.user_id == userId).OrderByDescending(x => x.income_date).ToListAsync();

        //    return Ok(new
        //    {
        //        message = "Incomes fetched successfully",
        //        data = incomes
        //    });
        //}




        [HttpPut("updateIncome/{id}")]
        public async Task<IActionResult> UpdateIncome(int id, IncomeDto incomeDto)
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

            var income = await _context.incomes.FirstOrDefaultAsync(x => x.id == id && x.user_id == userId);
            if (income == null)
            {
                return NotFound(new
                {
                    message = "Income not found."
                });
            }

            income.title = incomeDto.title;
            income.amount = incomeDto.amount;
            income.income_date = incomeDto.incomeDate;
            income.notes = incomeDto.notes;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Income Updated Successfully"
            });
        }

        [HttpDelete("deleteIncome/{id}")]
        public async Task<IActionResult> DeleteIncome(int id)
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

            var income = await _context.incomes.FirstOrDefaultAsync(x => x.id == id && x.user_id == userId);

            if (income == null)
            {
                return NotFound(new
                {
                    message = "Income not found."
                });
            }

            _context.incomes.Remove(income);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Income Deleted Successfully"
            });


        }

        [HttpGet("getIncomes")]
        public async Task<IActionResult> GetIncomes(string? search, int? month)
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

            var query = _context.incomes.Where(x => x.user_id == userId).AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                query = query.Where(x =>x.title.ToLower().Contains(search.ToLower()));
            }

            

            if (month.HasValue)
            {
                query = query.Where(x =>
                    x.income_date.Month == month.Value);
            }

            var incomes = await query
                .OrderByDescending(x => x.income_date)
                .ToListAsync();

            return Ok(new
            {
                message = "Incomes fetched successfully",
                data = incomes
            });
        }




    }
}
