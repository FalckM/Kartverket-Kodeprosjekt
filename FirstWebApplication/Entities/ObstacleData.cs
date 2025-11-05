using System.ComponentModel.DataAnnotations;

namespace FirstWebApplication.Entities
{
    public class ObstacleData
    {
        // Primary key
        [Key]
        public int Id { get; set; }

        // Obstacle information
        [Required(ErrorMessage = "Obstacle name is required")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters")]
        public string ObstacleName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Height is required")]
        [Range(0.1, 10000, ErrorMessage = "Height must be between 0.1 and 10000 meters")]
        public double ObstacleHeight { get; set; }

        [Required(ErrorMessage = "Description is required")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string ObstacleDescription { get; set; } = string.Empty;

        [Required(ErrorMessage = "Location is required")]
        public string ObstacleGeometry { get; set; } = string.Empty;

        [StringLength(50)]
        public string? ObstacleType { get; set; }

        // Registration information
        public DateTime RegisteredDate { get; set; } = DateTime.Now;

        [StringLength(100)]
        public string? RegisteredBy { get; set; }

        // Approval/Rejection fields
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