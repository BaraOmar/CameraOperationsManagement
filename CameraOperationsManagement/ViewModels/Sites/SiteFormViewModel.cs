using System.ComponentModel.DataAnnotations;

namespace CameraOperationsManagement.ViewModels.Sites
{
    public class SiteFormViewModel
    {
        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Location { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}