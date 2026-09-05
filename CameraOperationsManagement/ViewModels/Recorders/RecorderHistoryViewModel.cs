namespace CameraOperationsManagement.ViewModels.Recorders
{
    public class RecorderHistoryViewModel
    {
        public int RecorderId { get; set; }

        public string RecorderName { get; set; }
            = string.Empty;

        public string RecorderType { get; set; }
            = string.Empty;

        public string SiteId { get; set; }
            = string.Empty;

        public string SiteName { get; set; }
            = string.Empty;

        public string? NetworkSwitchName { get; set; }

        public bool IsActive { get; set; }


        public List<RecorderHistoryVisitViewModel> Visits
        { get; set; } = new();
    }


    public class RecorderHistoryVisitViewModel
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