namespace CameraOperationsManagement.ViewModels.RecorderHardDrives
{
    public class RecorderHardDriveListViewModel
    {
        public int RecorderId { get; set; }

        public string RecorderName { get; set; } = string.Empty;

        public string SiteName { get; set; } = string.Empty;

        public bool RecorderIsActive { get; set; }

        public List<RecorderHardDriveItemViewModel> HardDrives
        { get; set; } = new();

        public int TotalStorageGb =>
            HardDrives.Sum(h => h.CapacityGb);
    }


    public class RecorderHardDriveItemViewModel
    {
        public int Id { get; set; }

        public int CapacityGb { get; set; }

        public string? SerialNumber { get; set; }
    }
}