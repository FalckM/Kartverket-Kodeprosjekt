using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using NRLWebApp.Models.Entities;

namespace NRLWebApp.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }
        public DbSet<Organisasjon> Organisasjoner { get; set; }
        public DbSet<Status> Statuser { get; set; }
        public DbSet<Hinder> Hindre { get; set; }
        public DbSet<Behandling> Behandlinger { get; set; }
        public DbSet<HinderType> HinderTyper { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder); 

            builder.Entity<Behandling>()
                .HasOne(b => b.Hinder)
                .WithMany(h => h.Behandlinger)
                .HasForeignKey(b => b.HinderID)
                .OnDelete(DeleteBehavior.Restrict); 
        }
    }
}

