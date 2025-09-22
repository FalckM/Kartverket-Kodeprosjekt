using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models
{
    public class ObstacleData
    {
        [Required(ErrorMessage = "Field is required")]
        [MaxLength(100)]
        public string ObstacleName { get; set; }

        [Required(ErrorMessage = "Field is required")]
        [Range(0, 200)]
        public double ObstacleHeight { get; set; }

        [Required(ErrorMessage = "Field is required")]
        [MaxLength(1000)]
        public string ObstacleDescription { get; set; }

        [Required(ErrorMessage = "Field is required")]
        public string ObstacleGeometry { get; set; }
    }
}

