namespace CameraOperationsManagement.ViewModels.Dashboard
{
    public class DashboardViewModel
    {
        public int ActiveSites { get; set; }

        public int ActiveCameras { get; set; }

        public int ActiveWorkers { get; set; }

        public int TotalCameraVisits { get; set; }


        public List<RecentCameraVisitViewModel> RecentCameraVisits
        { get; set; } = new();


        public List<RecentSiteVisitViewModel> RecentSiteVisits
        { get; set; } = new();
    }


    public class RecentCameraVisitViewModel
    {
        public int VisitId { get; set; }

        public string CameraName { get; set; }
            = string.Empty;

        public string SiteName { get; set; }
            = string.Empty;

        public DateTime VisitDate { get; set; }

        public string Purpose { get; set; }
            = string.Empty;

        public string? MalfunctionType { get; set; }

        public string? RepairResult { get; set; }
    }


    public class RecentSiteVisitViewModel
    {
        public int VisitId { get; set; }

        public string SiteId { get; set; }
            = string.Empty;

        public string SiteName { get; set; }
            = string.Empty;

        public DateTime VisitDate { get; set; }

        public string Purpose { get; set; }
            = string.Empty;
    }
}