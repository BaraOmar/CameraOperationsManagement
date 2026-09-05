namespace CameraOperationsManagement.ViewModels.Visits
{
    public class VisitListItemViewModel
    {
        public int Id { get; set; }

        public DateTime VisitDate { get; set; }

        public string SiteId { get; set; }
            = string.Empty;

        public string SiteName { get; set; }
            = string.Empty;

        public string ComponentType { get; set; }
            = string.Empty;

        public string ComponentName { get; set; }
            = string.Empty;

        public string Purpose { get; set; }
            = string.Empty;

        public string? MalfunctionType { get; set; }

        public string? RepairResult { get; set; }

        public List<string> WorkerNames { get; set; }
            = new();
    }
}