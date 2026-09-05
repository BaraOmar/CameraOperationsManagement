using CameraOperationsManagement.Models.Enums;

namespace CameraOperationsManagement.ViewModels.Cameras
{
    public class CameraListItemViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Brand { get; set; }

        public string? Model { get; set; }

        public string? Type { get; set; }

        public CameraEnvironment Environment { get; set; }
        public string? IpAddress { get; set; }

        public string InstallationLocation { get; set; }
            = string.Empty;

        public int RecorderId { get; set; }

        public string RecorderName { get; set; }
            = string.Empty;

        public string SiteId { get; set; }
            = string.Empty;

        public string SiteName { get; set; }
            = string.Empty;

        public string? NetworkSwitchName { get; set; }

        public bool IsActive { get; set; }
    }
}