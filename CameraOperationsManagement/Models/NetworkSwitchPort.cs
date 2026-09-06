using CameraOperationsManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace CameraOperationsManagement.Models
{
    public class NetworkSwitchPort
    {
        public int Id { get; set; }


        public int NetworkSwitchId { get; set; }


        [Range(1, 32)]
        public int PortNumber { get; set; }


        public SwitchPortStatus Status { get; set; }
            = SwitchPortStatus.Available;


        // Null means no camera is currently connected.
        public int? CameraId { get; set; }


        public NetworkSwitch NetworkSwitch { get; set; }
            = null!;


        public Camera? Camera { get; set; }
    }
}