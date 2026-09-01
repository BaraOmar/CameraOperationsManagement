using System.ComponentModel.DataAnnotations;

namespace CameraOperationsManagement.Models
{
    public class RecorderHardDrive
    {
        public int Id { get; set; }


        [Required]
        public int RecorderId { get; set; }


        [Range(1, int.MaxValue)]
        [Display(Name = "Capacity (GB)")]
        public int CapacityGb { get; set; }


        [StringLength(100)]
        [Display(Name = "Serial Number")]
        public string? SerialNumber { get; set; }


        public Recorder Recorder { get; set; } = null!;
    }
}