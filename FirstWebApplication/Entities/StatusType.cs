using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Entities
{
    /// <summary>
    /// Lookup-tabell for status-typer
    /// 1 = Registered (Quick Register lagret)
    /// 2 = Pending (Venter på godkjenning)
    /// 3 = Approved (Godkjent)
    /// 4 = Rejected (Avvist)
    /// </summary>
    public class StatusType
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Name { get; set; } = string.Empty;

        [StringLength(255)]
        public string? Description { get; set; }

        // Navigation: En status-type kan ha mange ObstacleStatus-rader
        public ICollection<ObstacleStatus>? ObstacleStatuses { get; set; }
    }
}