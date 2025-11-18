using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Entities
{
    public class ObstacleData
    {
        [Key]
        public int Id { get; set; }

        // ALLE FELT ER REQUIRED IGJEN! ✅
        [Required(ErrorMessage = "Obstacle name is required")]
        [StringLength(100)]
        public string ObstacleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Height is required")]
        [Range(0.1, 10000)]
        public double ObstacleHeight { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000)]
        public string ObstacleDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required")]
        public string ObstacleGeometry { get; set; } = string.Empty;

        [StringLength(50)]
        public string? ObstacleType { get; set; }

        public DateTime RegisteredDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? RegisteredBy { get; set; }

        public bool IsApproved { get; set; } = false;
        public bool IsRejected { get; set; } = false;

        [StringLength(100)]
        public string? ApprovedBy { get; set; }

        public DateTime? ApprovedDate { get; set; }

        [StringLength(500)]
        public string? ApprovalComments { get; set; }

        [StringLength(100)]
        public string? RejectedBy { get; set; }

        public DateTime? RejectedDate { get; set; }

        [StringLength(500)]
        public string? RejectionReason { get; set; }
    }
}