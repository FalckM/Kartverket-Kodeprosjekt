using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Entities
{
    /// <summary>
    /// Behandlinger fra Registerfører
    /// Lagrer kun Approve/Reject-handlinger
    /// </summary>
    public class Behandling
    {
        [Key]
        public long Id { get; set; }

        // FK til Obstacle
        [Required]
        public long ObstacleId { get; set; }
        public Obstacle? Obstacle { get; set; }

        // FK til Registerfører som behandlet
        [Required]
        [StringLength(450)]
        public string RegisterforerUserId { get; set; } = string.Empty;
        public ApplicationUser? RegisterforerUser { get; set; }

        // FK til den ObstacleStatus som ble opprettet (Approved/Rejected status)
        [Required]
        public long StatusId { get; set; }
        public ObstacleStatus? Status { get; set; }

        // Hva ble gjort?
        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // "Approved" eller "Rejected"

        public DateTime ProcessedDate { get; set; } = DateTime.Now;

        [StringLength(255)]
        public string? Comments { get; set; }

        // Kun for avvisninger
        [StringLength(255)]
        public string? RejectionReason { get; set; }
    }
}