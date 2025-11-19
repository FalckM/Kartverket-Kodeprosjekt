using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models.Obstacle
{
    /// <summary>
    /// ViewModel for godkjenning av obstacle
    /// </summary>
    public class ApproveObstacleViewModel
    {
        [Required]
        public long ObstacleId { get; set; }

        [StringLength(255)]
        [Display(Name = "Comments (optional)")]
        public string? Comments { get; set; }
    }
}