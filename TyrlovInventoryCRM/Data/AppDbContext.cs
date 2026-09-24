using Microsoft.EntityFrameworkCore;
using TyrlovInventoryCRM.Models;

namespace TyrlovInventoryCRM.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Product> Products { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data Source=inventory.db");
        }
    }
}