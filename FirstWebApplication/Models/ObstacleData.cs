using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models
{
    public class ObstacleData
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Field is required")]
        [MaxLength(100)]
        public string ObstacleName { get; set; }

        [Required(ErrorMessage = "Field is required")]
        [Range(0, 200)]
        public double ObstacleHeight { get; set; }

        [Required(ErrorMessage = "Field is required")]
        [MaxLength(1000)]
        public string ObstacleDescription { get; set; }

        // Felt for å lagre GeoJSON (geografisk geometri) fra kartet.
        // Dette feltet lagrer koordinatene for det tegnede punktet eller linjen.
        [Required(ErrorMessage = "Field is required")]
        public string? ObstacleGeometry { get; set; }

        // tar tid og dato for registrering
        public DateTime RegisteredDate { get; set; } = DateTime.Now;

        // automatisk fanger hvem som sendte inn skjemaet

        [MaxLength(100)]
        [Display(Name = "Registered by (Email)")]
        public string? RegisteredBy { get; set; }
    }

}

