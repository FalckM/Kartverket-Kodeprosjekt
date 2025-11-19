using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Entities
{
    /// <summary>
    /// Lookup-tabell for hindertyper (Mast, Pole, Antenna, etc.)
    /// </summary>
    public class ObstacleType
    {
        [Key]
        public long Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        public decimal? MinHeight { get; set; }

        public decimal? MaxHeight { get; set; }

        // Navigation: En type kan ha mange obstacles
        public ICollection<Obstacle>? Obstacles { get; set; }
    }
}