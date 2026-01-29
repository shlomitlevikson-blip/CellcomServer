using Microsoft.EntityFrameworkCore;

namespace CellcomServer.Classes
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<LoginUser> LoginUsers { get; set; }

        public DbSet<Product> Products { get; set; }

        public DbSet<UserItem> AllUsers { get; set; }
    }
}
