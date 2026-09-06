using CameraOperationsManagement.Models.Enums;

namespace CameraOperationsManagement.ViewModels.NetworkSwitches
{
    public class NetworkSwitchDetailsViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string SiteId { get; set; } = string.Empty;

        public string SiteName { get; set; } = string.Empty;

        public int NumberOfPorts { get; set; }

        public bool IsActive { get; set; }


        public int AvailablePorts { get; set; }

        public int UsedPorts { get; set; }

        public int OutOfServicePorts { get; set; }


        public List<NetworkSwitchPortDetailsViewModel> Ports { get; set; }
            = new();

        public List<NetworkSwitchRecorderDetailsViewModel> Recorders { get; set; }
            = new();
    }


    public class NetworkSwitchPortDetailsViewModel
    {
        public int Id { get; set; }

        public int PortNumber { get; set; }

        public SwitchPortStatus Status { get; set; }

        public int? CameraId { get; set; }

        public string? CameraName { get; set; }

        public string? CameraIpAddress { get; set; }

        public string? CameraInstallationLocation { get; set; }

        public string? CameraDescription { get; set; }
    }


    public class NetworkSwitchRecorderDetailsViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}