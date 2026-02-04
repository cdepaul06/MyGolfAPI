using Microsoft.EntityFrameworkCore;
using MyGolfAPI.Entities;

namespace MyGolfAPI.Data
{
    public class MyGolfDbContext : DbContext
    {
        public MyGolfDbContext(DbContextOptions<MyGolfDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

    }
}
