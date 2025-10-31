using FirstWebApplication.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Data
{

    // ApplicationDbContext now inherits from IdentityDbContext instead of DbContext.
    // This gives us automatic support for users (Users), roles (Roles), login, etc.
    // IdentityDbContext creates tables like: AspNetUsers, AspNetRoles, AspNetUserRoles, etc.
    public class ApplicationDbContext : IdentityDbContext
    {

        // Constructor that receives configuration settings from Program.cs
        // DbContextOptions contains connection string and other settings
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet represents a table in the database.
        // DbSet<ObstacleData> = a table containing ObstacleData objects.
        // Entity Framework creates a table called "Obstacles" based on this DbSet.
        public DbSet<ObstacleData> Obstacles { get; set; } = null!;

        // OnModelCreating is called when the database is built for the first time.
        // Here we can configure tables, relationships, indexes, etc. 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // IMPORTANT: Must call base.OnModelCreating when using IdentityDbContext
            // This sets up all Identity tables (AspNetUsers, AspNetRoles, etc.)
            base.OnModelCreating(modelBuilder);

            // Explicitly set table name to "Obstacles" (Optional, but good practice)
            modelBuilder.Entity<ObstacleData>()
                .ToTable("Obstacles");

            // Sets default value if not set. 
            modelBuilder.Entity<ObstacleData>()
                .Property(o => o.RegisteredDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}