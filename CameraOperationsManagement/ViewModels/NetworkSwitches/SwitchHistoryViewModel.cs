namespace CameraOperationsManagement.ViewModels.NetworkSwitches
{
    public class SwitchHistoryViewModel
    {
        public int SwitchId { get; set; }

        public string SwitchName { get; set; }
            = string.Empty;

        public string SiteId { get; set; }
            = string.Empty;

        public string SiteName { get; set; }
            = string.Empty;

        public bool IsActive { get; set; }


        public List<SwitchHistoryVisitViewModel> Visits
        { get; set; } = new();
    }


    public class SwitchHistoryVisitViewModel
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