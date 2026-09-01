namespace CameraOperationsManagement.ViewModels.SiteVisits
{
    public class SiteVisitListItemViewModel
    {
        public int Id { get; set; }

        public string SiteId { get; set; } = string.Empty;

        public string SiteName { get; set; } = string.Empty;

        public DateTime VisitDate { get; set; }

        public string Purpose { get; set; } = string.Empty;

        public List<string> WorkerNames { get; set; } = new();
    }
}