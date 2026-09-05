namespace CameraOperationsManagement.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public int ActiveSites { get; set; }

        public int ActiveCameras { get; set; }

        public int ActiveWorkers { get; set; }

        public int TotalVisits { get; set; }


        public List<RecentVisitViewModel> RecentVisits
        { get; set; } = new();
    }


    public class RecentVisitViewModel
    {
        public int VisitId { get; set; }

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
    }
}