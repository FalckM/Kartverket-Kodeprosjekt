using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Entities
{
    /// <summary>
    /// Hovedtabell for obstacles (hindre)
    /// Inneholder kun kjernedata, IKKE status-informasjon
    /// </summary>
    public class Obstacle
    {
        [Key]
        public long Id { get; set; }

        // FK til ObstacleType
        public long? ObstacleTypeId { get; set; }
        public ObstacleType? ObstacleType { get; set; }

        // FK til bruker som registrerte (pilot)
        [Required]
        [StringLength(450)]
        public string RegisteredByUserId { get; set; } = string.Empty;
        public ApplicationUser? RegisteredByUser { get; set; }

        // FK til nåværende status (nullable - settes etter første status)
        public long? CurrentStatusId { get; set; }
        public ObstacleStatus? CurrentStatus { get; set; }

        // Kjernedata
        [StringLength(100)]
        public string? Name { get; set; }

        [Range(0.1, 10000)]
        public decimal? Height { get; set; }

        [StringLength(1000)]
        public string? Description { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty; // JSON eller WKT

        public DateTime RegisteredDate { get; set; } = DateTime.Now;

        // Navigation: Et obstacle har mange status-endringer
        public ICollection<ObstacleStatus>? StatusHistory { get; set; }

        // Navigation: Et obstacle kan ha mange behandlinger
        public ICollection<Behandling>? Behandlinger { get; set; }
    }
}