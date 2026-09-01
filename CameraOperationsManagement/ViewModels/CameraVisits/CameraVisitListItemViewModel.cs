namespace CameraOperationsManagement.ViewModels.CameraVisits
{
    public class CameraVisitListItemViewModel
    {
        public int Id { get; set; }

        public int CameraId { get; set; }

        public string CameraName { get; set; }
            = string.Empty;

        public string RecorderName { get; set; }
            = string.Empty;

        public string SiteName { get; set; }
            = string.Empty;

        public DateTime VisitDate { get; set; }

        public string Purpose { get; set; }
            = string.Empty;

        public string? MalfunctionType { get; set; }

        public string? MalfunctionDescription { get; set; }

        public string? RepairWorkPerformed { get; set; }

        public string? RepairResult { get; set; }

        public List<string> WorkerNames { get; set; }
            = new();
    }
}