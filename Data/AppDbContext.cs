using Microsoft.EntityFrameworkCore;
using ExpenseTracker.API.Models;

namespace ExpenseTracker.API.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<User> users { get; set; }
        public DbSet<Expense> expenses { get; set; }
        public DbSet<Income> incomes { get; set; }
        public DbSet<Budget> budgets { get; set; }
        }
}
