using System.ComponentModel.DataAnnotations;

namespace CameraOperationsManagement.Models
{
    public class Site
    {
        [Key]
        [Required]
        [StringLength(50)]
        [Display(Name = "Site ID")]
        public string Id { get; set; } = string.Empty;


        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;


        [StringLength(200)]
        public string? Location { get; set; }


        [StringLength(1000)]
        public string? Notes { get; set; }


        public bool IsActive { get; set; } = true;
    }
}