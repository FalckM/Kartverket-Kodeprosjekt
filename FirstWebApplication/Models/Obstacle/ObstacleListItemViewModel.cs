namespace FirstWebApplication.Models.Obstacle
{
    /// <summary>
    /// ViewModel for obstacles i lister
    /// </summary>
    public class ObstacleListItemViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Height { get; set; }
        public string? Type { get; set; }
        public string Location { get; set; } = string.Empty;
        public DateTime RegisteredDate { get; set; }
        public string RegisteredBy { get; set; } = string.Empty;

        // Status info
        public string CurrentStatus { get; set; } = string.Empty; // "Registered", "Pending", "Approved", "Rejected"
        public string? StatusName { get; set; } // Samme som CurrentStatus, men nullable
        public bool IsIncomplete { get; set; } // For Quick Registrations
        public bool IsPending { get; set; }
        public bool IsApproved { get; set; }
        public bool IsRejected { get; set; }

        // Approval/Rejection info
        public string? ProcessedBy { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public string? RejectionReason { get; set; }
    }
}