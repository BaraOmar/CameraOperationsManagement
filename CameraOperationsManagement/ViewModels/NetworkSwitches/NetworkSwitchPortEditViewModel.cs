using CameraOperationsManagement.Models.Enums;

namespace CameraOperationsManagement.ViewModels.NetworkSwitches
{
    public class NetworkSwitchPortEditViewModel
    {
        public int Id { get; set; }

        public int PortNumber { get; set; }

        public SwitchPortStatus Status { get; set; }

        public string? CameraName { get; set; }
    }
}