using FirstWebApplication.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Data
{
    // VIKTIG: Bruker nå ApplicationUser i stedet for IdentityUser
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // NYE NORMALISERTE TABELLER
        public DbSet<Organisasjon> Organisasjoner { get; set; } = null!;
        public DbSet<ObstacleType> ObstacleTypes { get; set; } = null!;
        public DbSet<StatusType> StatusTypes { get; set; } = null!;
        public DbSet<Obstacle> Obstacles { get; set; } = null!;
        public DbSet<ObstacleStatus> ObstacleStatuses { get; set; } = null!;
        public DbSet<Behandling> Behandlinger { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // ORGANISASJON
            // ==========================================
            modelBuilder.Entity<Organisasjon>()
                .ToTable("Organisasjoner");

            modelBuilder.Entity<Organisasjon>()
                .Property(o => o.CreatedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // ==========================================
            // APPLICATION USER (utvider IdentityUser)
            // ==========================================
            modelBuilder.Entity<ApplicationUser>()
                .HasOne(u => u.Organisasjon)
                .WithMany(o => o.Users)
                .HasForeignKey(u => u.OrganisasjonId)
                .OnDelete(DeleteBehavior.SetNull);

            // ==========================================
            // OBSTACLE TYPE
            // ==========================================
            modelBuilder.Entity<ObstacleType>()
                .ToTable("ObstacleTypes");

            // ==========================================
            // STATUS TYPE
            // ==========================================
            modelBuilder.Entity<StatusType>()
                .ToTable("StatusTypes");

            // Seed StatusTypes (standard verdier)
            modelBuilder.Entity<StatusType>().HasData(
                new StatusType { Id = 1, Name = "Registered", Description = "Quick Register saved - incomplete" },
                new StatusType { Id = 2, Name = "Pending", Description = "Awaiting approval from Registerfører" },
                new StatusType { Id = 3, Name = "Approved", Description = "Approved by Registerfører" },
                new StatusType { Id = 4, Name = "Rejected", Description = "Rejected by Registerfører" }
            );

            // ==========================================
            // OBSTACLE
            // ==========================================
            modelBuilder.Entity<Obstacle>()
                .ToTable("Obstacles");

            modelBuilder.Entity<Obstacle>()
                .Property(o => o.RegisteredDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // FK: Obstacle -> ObstacleType
            modelBuilder.Entity<Obstacle>()
                .HasOne(o => o.ObstacleType)
                .WithMany(ot => ot.Obstacles)
                .HasForeignKey(o => o.ObstacleTypeId)
                .OnDelete(DeleteBehavior.SetNull);

            // FK: Obstacle -> RegisteredByUser
            modelBuilder.Entity<Obstacle>()
                .HasOne(o => o.RegisteredByUser)
                .WithMany()
                .HasForeignKey(o => o.RegisteredByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: Obstacle -> CurrentStatus (nullable, settes senere)
            modelBuilder.Entity<Obstacle>()
                .HasOne(o => o.CurrentStatus)
                .WithMany()
                .HasForeignKey(o => o.CurrentStatusId)
                .OnDelete(DeleteBehavior.SetNull);

            // ==========================================
            // OBSTACLE STATUS
            // ==========================================
            modelBuilder.Entity<ObstacleStatus>()
                .ToTable("ObstacleStatuses");

            modelBuilder.Entity<ObstacleStatus>()
                .Property(os => os.ChangedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // FK: ObstacleStatus -> Obstacle
            modelBuilder.Entity<ObstacleStatus>()
                .HasOne(os => os.Obstacle)
                .WithMany(o => o.StatusHistory)
                .HasForeignKey(os => os.ObstacleId)
                .OnDelete(DeleteBehavior.Cascade);

            // FK: ObstacleStatus -> StatusType
            modelBuilder.Entity<ObstacleStatus>()
                .HasOne(os => os.StatusType)
                .WithMany(st => st.ObstacleStatuses)
                .HasForeignKey(os => os.StatusTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: ObstacleStatus -> ChangedByUser
            modelBuilder.Entity<ObstacleStatus>()
                .HasOne(os => os.ChangedByUser)
                .WithMany()
                .HasForeignKey(os => os.ChangedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // ==========================================
            // BEHANDLING
            // ==========================================
            modelBuilder.Entity<Behandling>()
                .ToTable("Behandlinger");

            modelBuilder.Entity<Behandling>()
                .Property(b => b.ProcessedDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");

            // FK: Behandling -> Obstacle
            modelBuilder.Entity<Behandling>()
                .HasOne(b => b.Obstacle)
                .WithMany(o => o.Behandlinger)
                .HasForeignKey(b => b.ObstacleId)
                .OnDelete(DeleteBehavior.Cascade);

            // FK: Behandling -> RegisterforerUser
            modelBuilder.Entity<Behandling>()
                .HasOne(b => b.RegisterforerUser)
                .WithMany()
                .HasForeignKey(b => b.RegisterforerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            // FK: Behandling -> Status
            modelBuilder.Entity<Behandling>()
                .HasOne(b => b.Status)
                .WithMany(os => os.Behandlinger)
                .HasForeignKey(b => b.StatusId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}