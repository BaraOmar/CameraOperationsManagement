namespace CameraOperationsManagement.ViewModels.Workers
{
    public class WorkerHistoryViewModel
    {
        public int WorkerId { get; set; }

        public string WorkerName { get; set; }
            = string.Empty;

        public bool IsActive { get; set; }


        public List<WorkerSiteVisitHistoryViewModel> SiteVisits
        { get; set; } = new();


        public List<WorkerCameraVisitHistoryViewModel> CameraVisits
        { get; set; } = new();
    }


    public class WorkerSiteVisitHistoryViewModel
    {
        public int VisitId { get; set; }

        public string SiteId { get; set; }
            = string.Empty;

        public string SiteName { get; set; }
            = string.Empty;

        public DateTime VisitDate { get; set; }

        public string Purpose { get; set; }
            = string.Empty;

        public string? Notes { get; set; }
    }


    public class WorkerCameraVisitHistoryViewModel
    {
        public int VisitId { get; set; }

        public int CameraId { get; set; }

        public string CameraName { get; set; }
            = string.Empty;

        public string SiteName { get; set; }
            = string.Empty;

        public string RecorderName { get; set; }
            = string.Empty;

        public DateTime VisitDate { get; set; }

        public string Purpose { get; set; }
            = string.Empty;

        public string? MalfunctionType { get; set; }

        public string? MalfunctionDescription { get; set; }

        public string? RepairWorkPerformed { get; set; }

        public string? RepairResult { get; set; }

        public string? Notes { get; set; }
    }
}