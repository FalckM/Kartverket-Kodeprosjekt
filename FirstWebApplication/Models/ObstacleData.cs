using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models
{
    public class ObstacleData
    {


        // Bruker Key for å fortelle Entity Framework at dette er primærnøkkelen.
        // Denne vil automatisk få verdier ved innsending og bruk av AUTO_INCREMENT i databasen.
        [Key]
        public int Id { get; set; }


        [Required(ErrorMessage = "Field is required")]
        [MaxLength(100)]
        [Display(Name = "Obstacle Name")]
        public string ObstacleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Field is required")]
        [Range(0, 200)]
        [Display(Name = "Obstacle Height (meters)")]
        public double ObstacleHeight { get; set; }

        [Required(ErrorMessage = "Field is required")]
        [MaxLength(1000)]
        [Display(Name = "Obstacle Description")]
        public string ObstacleDescription { get; set; } = string.Empty;

        // Felt for å lagre GeoJSON (geografisk geometri) fra kartet.
        // Dette feltet lagrer koordinatene for det tegnede punktet eller linjen.
        [Required(ErrorMessage = "Field is required")]
        [Display(Name = "Obstacle Geometry (GeoJSON)")]
        public string ObstacleGeometry { get; set; } = string.Empty;

        // Automatisk tidspunkt for når hindringen ble registrert.
        // Date.Time setter dagens dato og klokkeslett som standardverdi.
        [Display(Name = "Registration Time")]
        public DateTime RegisteredDate { get; set; } = DateTime.Now;

        // Automatisk felt for å lagre hvem som registrerte hindringen.
        // Kan være null hvis ikke oppgitt.
        [MaxLength(100)]
        [Display(Name = "Registered By (Email)")]
        public string? RegisteredBy { get; set; }

        // Type hinder (valgfritt felt)
        [MaxLength(50)]
        public string? ObstacleType { get; set; }


    }
}

