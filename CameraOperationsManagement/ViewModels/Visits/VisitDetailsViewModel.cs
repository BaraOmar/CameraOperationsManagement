namespace CameraOperationsManagement.ViewModels.Visits
{
    public class VisitDetailsViewModel
    {
        public int Id { get; set; }

        public string SiteId { get; set; }
            = string.Empty;

        public string SiteName { get; set; }
            = string.Empty;

        public string ComponentType { get; set; }
            = string.Empty;

        public string ComponentName { get; set; }
            = string.Empty;

        public DateTime VisitDate { get; set; }

        public string Purpose { get; set; }
            = string.Empty;

        public List<string> WorkerNames { get; set; }
            = new();

        public string? MalfunctionType { get; set; }

        public string? MalfunctionDescription { get; set; }

        public string? RepairWorkPerformed { get; set; }

        public string? RepairResult { get; set; }

        public string? Notes { get; set; }
    }
}