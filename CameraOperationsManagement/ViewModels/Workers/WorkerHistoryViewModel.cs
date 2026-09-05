using CameraOperationsManagement.Models.Enums;

namespace CameraOperationsManagement.ViewModels.Workers
{
    public class WorkerHistoryViewModel
    {
        public int WorkerId { get; set; }

        public string FirstName { get; set; }
            = string.Empty;

        public string SecondName { get; set; }
            = string.Empty;

        public string LastName { get; set; }
            = string.Empty;

        public bool IsActive { get; set; }


        public string FullName =>
            $"{FirstName} {SecondName} {LastName}";


        public List<WorkerHistoryVisitViewModel> Visits
        { get; set; } = new();
    }


    public class WorkerHistoryVisitViewModel
    {
        public int VisitId { get; set; }

        public DateTime VisitDate { get; set; }


        public string SiteId { get; set; }
            = string.Empty;

        public string SiteName { get; set; }
            = string.Empty;


        public VisitComponentType ComponentType { get; set; }

        public string ComponentName { get; set; }
            = string.Empty;


        public string Purpose { get; set; }
            = string.Empty;


        public string? MalfunctionType { get; set; }

        public string? MalfunctionDescription { get; set; }

        public string? RepairWorkPerformed { get; set; }

        public string? RepairResult { get; set; }

        public string? Notes { get; set; }
    }
}