using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Models.Obstacle
{
    /// <summary>
    /// ViewModel for avvisning av obstacle
    /// </summary>
    public class RejectObstacleViewModel
    {
        [Required]
        public long ObstacleId { get; set; }

        [Required(ErrorMessage = "Please provide a reason for rejection")]
        [StringLength(255)]
        [Display(Name = "Rejection Reason")]
        public string RejectionReason { get; set; } = string.Empty;

        [StringLength(255)]
        [Display(Name = "Additional Comments (optional)")]
        public string? Comments { get; set; }
    }
}