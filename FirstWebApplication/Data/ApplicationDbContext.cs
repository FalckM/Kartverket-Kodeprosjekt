using FirstWebApplication.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Data
{
    // Inherit from IdentityDbContext to get user/role support
    // Identity automatically creates these tables:
    // - AspNetUsers (for user accounts)
    // - AspNetRoles (for roles like "Admin", "User")
    // - AspNetUserRoles (linking users to roles)
    // - AspNetUserClaims, AspNetUserLogins, AspNetUserTokens, AspNetRoleClaims
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // Your custom tables
        public DbSet<ObstacleData> Obstacles { get; set; } = null!;

        // NOTE: You do NOT need a custom UserRoles table!
        // Identity's AspNetUserRoles table handles this automatically.
        // To work with user roles, use UserManager and RoleManager services.

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