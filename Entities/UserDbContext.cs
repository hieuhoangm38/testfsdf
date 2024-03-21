using Microsoft.EntityFrameworkCore;

namespace WebApplication1.Entities
{
    public class UserDbContext : DbContext
    {
        public UserDbContext(DbContextOptions<UserDbContext> options) : base(options) { }


        public DbSet<User> Users { get; set; }
    }
}
