using FirstWebApplication.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Data
{
    // Inherit from IdentityDbContext to get user/role support
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Your obstacles table
        public DbSet<ObstacleData> Obstacles { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // MUST call this first to set up Identity tables
            base.OnModelCreating(modelBuilder);

            // Configure Obstacles table
            modelBuilder.Entity<ObstacleData>()
                .ToTable("Obstacles");

            modelBuilder.Entity<ObstacleData>()
                .Property(o => o.RegisteredDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}