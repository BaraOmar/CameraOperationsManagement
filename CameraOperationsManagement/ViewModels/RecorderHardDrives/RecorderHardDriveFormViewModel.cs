using System.ComponentModel.DataAnnotations;

namespace CameraOperationsManagement.ViewModels.RecorderHardDrives
{
    public class RecorderHardDriveFormViewModel
    {
        public int Id { get; set; }

        public int RecorderId { get; set; }


        [Required]
        [Range(1, int.MaxValue)]
        [Display(Name = "Capacity (GB)")]
        public int CapacityGb { get; set; }


        [StringLength(100)]
        [Display(Name = "Serial Number")]
        public string? SerialNumber { get; set; }
    }
}