using CameraOperationsManagement.Models;
using System.ComponentModel.DataAnnotations;

namespace CameraOperationsManagement.ViewModels.Recorders
{
    public class RecorderFormViewModel
    {
        public int Id { get; set; }


        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;


        [Required]
        [Display(Name = "Recorder Type")]
        public RecorderType? Type { get; set; }


        [Required]
        [Display(Name = "Site")]
        public string SiteId { get; set; } = string.Empty;


        [Display(Name = "Network Switch")]
        public int? NetworkSwitchId { get; set; }


        [Display(Name = "Has Storage")]
        public bool HasStorage { get; set; }
    }
}