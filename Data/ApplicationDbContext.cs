using Microsoft.EntityFrameworkCore;
using Playground.Entities;

namespace Playground.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<ProductEntity> Products { get; set; } = null!;
        public DbSet<UserEntity> Users { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<ProductEntity>(b =>
            {
                b.HasKey(p => p.Id);
                b.Property(p => p.Name).IsRequired();
                b.Property(p => p.Price).HasPrecision(18, 2);
            });

            modelBuilder.Entity<UserEntity>(b =>
            {
                b.HasKey(u => u.Id);
                b.HasIndex(u => u.Username).IsUnique();
                b.Property(u => u.Username).IsRequired();
                b.Property(u => u.PasswordHash).IsRequired();
            });
        }
    }
}
