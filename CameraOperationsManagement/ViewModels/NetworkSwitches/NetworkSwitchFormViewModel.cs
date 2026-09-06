using System.ComponentModel.DataAnnotations;

namespace CameraOperationsManagement.ViewModels.NetworkSwitches
{
    public class NetworkSwitchFormViewModel
    {
        public int Id { get; set; }


        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;


        [Required]
        [Display(Name = "Site")]
        public string SiteId { get; set; } = string.Empty;


        [Required]
        [Display(Name = "Number of Ports")]
        public int? NumberOfPorts { get; set; }

            public List<NetworkSwitchPortEditViewModel> Ports { get; set; }
    = new();
    }
}