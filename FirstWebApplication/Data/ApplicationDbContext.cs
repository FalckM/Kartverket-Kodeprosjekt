using FirstWebApplication.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // TO TABELLER! ✅
        public DbSet<QuickRegistration> QuickRegistrations { get; set; } = null!;
        public DbSet<ObstacleData> Obstacles { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // QuickRegistrations table
            modelBuilder.Entity<QuickRegistration>()
                .ToTable("QuickRegistrations");

            modelBuilder.Entity<QuickRegistration>()
                .Property(q => q.RegisteredDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // Obstacles table
            modelBuilder.Entity<ObstacleData>()
                .ToTable("Obstacles");

            modelBuilder.Entity<ObstacleData>()
                .Property(o => o.RegisteredDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}