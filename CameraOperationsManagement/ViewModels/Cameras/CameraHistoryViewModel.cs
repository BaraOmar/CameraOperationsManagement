namespace CameraOperationsManagement.ViewModels.Cameras
{
    public class CameraHistoryViewModel
    {
        public int CameraId { get; set; }

        public string CameraName { get; set; }
            = string.Empty;

        public string? Brand { get; set; }

        public string? Model { get; set; }

        public string? SerialNumber { get; set; }

        public string? Type { get; set; }

        public string? IpAddress { get; set; }

        public string InstallationLocation { get; set; }
            = string.Empty;

        public DateTime? InstallationDate { get; set; }

        public string RecorderName { get; set; }
            = string.Empty;

        public string SiteId { get; set; }
            = string.Empty;

        public string SiteName { get; set; }
            = string.Empty;

        public bool IsActive { get; set; }


        public List<CameraHistoryVisitViewModel> Visits
        { get; set; } = new();
    }


    public class CameraHistoryVisitViewModel
    {
        public int VisitId { get; set; }

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