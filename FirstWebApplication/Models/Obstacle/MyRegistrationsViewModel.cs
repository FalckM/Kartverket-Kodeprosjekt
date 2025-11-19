namespace FirstWebApplication.Models.Obstacle
{
    /// <summary>
    /// ViewModel for "My Registrations" siden
    /// </summary>
    public class MyRegistrationsViewModel
    {
        public List<IncompleteQuickRegItem> IncompleteQuickRegs { get; set; } = new();
        public List<ObstacleListItemViewModel> PendingObstacles { get; set; } = new();
        public List<ObstacleListItemViewModel> ApprovedObstacles { get; set; } = new();
        public List<ObstacleListItemViewModel> RejectedObstacles { get; set; } = new();

        // Counts for statistics
        public int IncompleteCount => IncompleteQuickRegs.Count;
        public int PendingCount => PendingObstacles.Count;
        public int ApprovedCount => ApprovedObstacles.Count;
        public int RejectedCount => RejectedObstacles.Count;
    }

    public class IncompleteQuickRegItem
    {
        public long Id { get; set; }
        public string Location { get; set; } = string.Empty;
        public DateTime RegisteredDate { get; set; }
    }
}