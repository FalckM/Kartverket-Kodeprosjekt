namespace FirstWebApplication.Models.Obstacle
{
    /// <summary>
    /// ViewModel for detaljert visning av et obstacle
    /// </summary>
    public class ObstacleDetailsViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal Height { get; set; }
        public string Description { get; set; } = string.Empty;
        public string? Type { get; set; }
        public string Location { get; set; } = string.Empty;
        public DateTime RegisteredDate { get; set; }
        public string RegisteredBy { get; set; } = string.Empty;

        // Current status
        public string CurrentStatus { get; set; } = string.Empty;
        public bool IsPending { get; set; }
        public bool IsApproved { get; set; }
        public bool IsRejected { get; set; }

        // Status history
        public List<StatusHistoryItem> StatusHistory { get; set; } = new();

        // Latest approval/rejection
        public string? ProcessedBy { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public string? ProcessComments { get; set; }
        public string? RejectionReason { get; set; }
    }

    public class StatusHistoryItem
    {
        public string Status { get; set; } = string.Empty;
        public string ChangedBy { get; set; } = string.Empty;
        public DateTime ChangedDate { get; set; }
        public string? Comments { get; set; }
    }
}