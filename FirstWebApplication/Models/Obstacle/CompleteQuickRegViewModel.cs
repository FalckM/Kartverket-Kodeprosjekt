using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models.Obstacle
{
    /// <summary>
    /// ViewModel for å fullføre en Quick Registration
    /// </summary>
    public class CompleteQuickRegViewModel
    {
        public long ObstacleId { get; set; }

        [Required(ErrorMessage = "Obstacle name is required")]
        [StringLength(100)]
        [Display(Name = "Obstacle Name")]
        public string ObstacleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Height is required")]
        [Range(0.1, 10000, ErrorMessage = "Height must be between 0.1 and 10000 meters")]
        [Display(Name = "Height (meters)")]
        public decimal ObstacleHeight { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000)]
        [Display(Name = "Description")]
        public string ObstacleDescription { get; set; } = string.Empty;

        [StringLength(50)]
        [Display(Name = "Obstacle Type")]
        public string? ObstacleType { get; set; }

        // Read-only fields (satt av controller)
        public string ObstacleGeometry { get; set; } = string.Empty;
        public DateTime RegisteredDate { get; set; }
        public string? RegisteredBy { get; set; }
    }
}