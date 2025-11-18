using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Entities
{
    /// Midlertidig registrering fra Quick Register.
    /// Inneholder bare GPS-posisjon.
    public class QuickRegistration
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Location is required")]
        public string ObstacleGeometry { get; set; } = string.Empty;

        public DateTime RegisteredDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? RegisteredBy { get; set; }

        // Er denne fullført?
        public bool IsCompleted { get; set; } = false;

        // Referanse til Obstacle hvis fullført
        public int? CompletedObstacleId { get; set; }
    }
}