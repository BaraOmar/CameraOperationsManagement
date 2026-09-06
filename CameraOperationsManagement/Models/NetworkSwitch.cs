using System.ComponentModel.DataAnnotations;

namespace CameraOperationsManagement.Models
{
    public class NetworkSwitch
    {
        public int Id { get; set; }


        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;


        [Required]
        [StringLength(50)]
        [Display(Name = "Site")]
        public string SiteId { get; set; } = string.Empty;


        public bool IsActive { get; set; } = true;


        public Site Site { get; set; } = null!;

        public int NumberOfPorts { get; set; }

        public ICollection<NetworkSwitchPort> Ports { get; set; }
            = new List<NetworkSwitchPort>();
    }
}