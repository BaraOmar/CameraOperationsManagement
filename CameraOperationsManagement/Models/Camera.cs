using CameraOperationsManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace CameraOperationsManagement.Models
{
    public class Camera
    {
        public int Id { get; set; }


        [Required]
        [StringLength(100)]
        [Display(Name = "Camera Name")]
        public string Name { get; set; } = string.Empty;


        [StringLength(100)]
        public string? Brand { get; set; }


        [StringLength(100)]
        public string? Model { get; set; }


        [StringLength(100)]
        [Display(Name = "Serial Number")]
        public string? SerialNumber { get; set; }


        [StringLength(100)]
        [Display(Name = "Camera Type")]
        public string? Type { get; set; }

        [Required]
        [Display(Name = "Camera Environment")]
        public CameraEnvironment Environment { get; set; }

        [StringLength(45)]
        [Display(Name = "IP Address")]
        public string? IpAddress { get; set; }


        [Required]
        [StringLength(200)]
        [Display(Name = "Installation Location")]
        public string InstallationLocation { get; set; }
            = string.Empty;


        [Display(Name = "Installation Date")]
        [DataType(DataType.Date)]
        public DateTime? InstallationDate { get; set; }


        [StringLength(1000)]
        public string? Notes { get; set; }


        [Required]
        [Display(Name = "Recorder")]
        public int RecorderId { get; set; }


        [Display(Name = "Network Switch")]
        public int? NetworkSwitchId { get; set; }


        public bool IsActive { get; set; } = true;


        public Recorder Recorder { get; set; } = null!;

        public NetworkSwitch? NetworkSwitch { get; set; }
    }
}