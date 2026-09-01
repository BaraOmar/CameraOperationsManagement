using CameraOperationsManagement.Models;

namespace CameraOperationsManagement.ViewModels.Recorders
{
    public class RecorderListItemViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public RecorderType Type { get; set; }

        public string SiteId { get; set; } = string.Empty;

        public string SiteName { get; set; } = string.Empty;

        public string? NetworkSwitchName { get; set; }

        public bool HasStorage { get; set; }

        public int TotalStorageGb { get; set; }

        public bool IsActive { get; set; }
    }
}