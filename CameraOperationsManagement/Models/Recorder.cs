using System.ComponentModel.DataAnnotations;

namespace CameraOperationsManagement.Models
{
    public class Recorder
    {
        public int Id { get; set; }


        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;


        [Required]
        public RecorderType Type { get; set; }


        [Required]
        [StringLength(50)]
        public string SiteId { get; set; } = string.Empty;


        public int? NetworkSwitchId { get; set; }


        public bool HasStorage { get; set; }


        public bool IsActive { get; set; } = true;


        public Site Site { get; set; } = null!;

        public NetworkSwitch? NetworkSwitch { get; set; }

        public ICollection<RecorderHardDrive> HardDrives
        { get; set; } = new List<RecorderHardDrive>();
    }
}