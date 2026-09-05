using CameraOperationsManagement.Models.Enums;

namespace CameraOperationsManagement.ViewModels.Sites
{
    public class SiteHistoryViewModel
    {
        public string SiteId { get; set; } = string.Empty;

        public string SiteName { get; set; } = string.Empty;

        public string? Location { get; set; }

        public string? Notes { get; set; }

        public bool IsActive { get; set; }


        public List<SiteHistoryVisitViewModel> Visits
        { get; set; } = new();


        // Keep these temporarily because the current
        // Site PDF service still uses them.
        public List<SiteHistorySiteVisitViewModel> SiteVisits
        { get; set; } = new();

        public List<SiteHistoryCameraVisitViewModel> CameraVisits
        { get; set; } = new();
    }


    public class SiteHistoryVisitViewModel
    {
        public int VisitId { get; set; }

        public DateTime VisitDate { get; set; }

        public VisitComponentType ComponentType { get; set; }

        public string ComponentName { get; set; }
            = string.Empty;

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


    // Temporary legacy classes for the existing PDF.
    public class SiteHistorySiteVisitViewModel
    {
        public int VisitId { get; set; }

        public DateTime VisitDate { get; set; }

        public string Purpose { get; set; } = string.Empty;

        public string? Notes { get; set; }

        public List<string> WorkerNames { get; set; }
            = new();
    }


    public class SiteHistoryCameraVisitViewModel
    {
        public int VisitId { get; set; }

        public int CameraId { get; set; }

        public string CameraName { get; set; }
            = string.Empty;

        public DateTime VisitDate { get; set; }

        public string Purpose { get; set; }
            = string.Empty;

        public string? MalfunctionType { get; set; }

        public string? MalfunctionDescription { get; set; }

        public string? RepairWorkPerformed { get; set; }

        public string? RepairResult { get; set; }

        public string? Notes { get; set; }

        public List<string> WorkerNames { get; set; }
            = new();
    }
}