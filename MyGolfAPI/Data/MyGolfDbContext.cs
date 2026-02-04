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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Auth0Sub)
                .IsUnique();
        }
    }
}
