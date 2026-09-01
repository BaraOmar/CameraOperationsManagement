using System.ComponentModel.DataAnnotations;

namespace CameraOperationsManagement.ViewModels.Sites
{
    public class CreateSiteViewModel : SiteFormViewModel
    {
        [Required]
        [StringLength(50)]
        [Display(Name = "Site ID")]
        public string Id { get; set; } = string.Empty;
    }
}