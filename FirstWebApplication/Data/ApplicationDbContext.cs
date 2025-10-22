using FirstWebApplication.Models;
using Microsoft.EntityFrameworkCore;

namespace FirstWebApplication.Data
{

    // DbContext er "broen" melllom C#-koden og databasen.
    // Den håndterer alle database-operasjoner (CRUD - Create, Read, Update, Delete).
    public class ApplicationDbContext : DbContext
    {

        // Constructor som tar imot konfigurasjonsinnstillinger fra Program.cs
        // DbContextOptions inneholder connection string og andre innstillinger
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSet representerer en tabell i databasen.
        // DbSet<ObstacleData> = en tabell som inneholder ObstacleData-objekter.
        // Entity Framework lager en tabell kalt "Obstacles" basert på denne DbSet-en.
        public DbSet<ObstacleData> Obstacles { get; set; } = null!;

        // OnModelCreating kjøres når databasen bygges for første gang.
        // Her kan vi konfigurere tabeller, relasjoner, indesker, osv. 
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Setter eksplisitt tabellnavn til "Obstacles" (Valgfritt, men god praksis)
            modelBuilder.Entity<ObstacleData>()
                .ToTable("Obstacles");

            // Sikrer at ObstacleName er unikt i databasen og unngår duplikater.
            // Dette lager en uniqe index i databasen.
            modelBuilder.Entity<ObstacleData>()
                .HasIndex(o => o.ObstacleName)
                .IsUnique();

            // Setter standardverdi hvis den ikke er satt. 
            modelBuilder.Entity<ObstacleData>()
                .Property(o => o.RegisteredDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP");
        }
    }
}
