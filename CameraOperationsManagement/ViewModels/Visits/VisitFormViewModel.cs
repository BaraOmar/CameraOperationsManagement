using CameraOperationsManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace CameraOperationsManagement.ViewModels.Visits
{
    public class VisitFormViewModel
    {
        public int Id { get; set; }


        [Required(ErrorMessage = "Please select a site.")]
        [Display(Name = "Site")]
        public string SiteId { get; set; }
            = string.Empty;


        [Required(ErrorMessage = "Please select a component type.")]
        [Display(Name = "Component Type")]
        public VisitComponentType? ComponentType
        { get; set; }


        [Required(ErrorMessage = "Please select a component.")]
        [Range(1, int.MaxValue)]
        [Display(Name = "Component")]
        public int? ComponentId { get; set; }


        [Required]
        [Display(Name = "Visit Date")]
        public DateTime VisitDate { get; set; }
            = DateTime.Now;


        [Required]
        [StringLength(300)]
        public string Purpose { get; set; }
            = string.Empty;


        [StringLength(100)]
        [Display(Name = "Malfunction Type")]
        public string? MalfunctionType { get; set; }


        [StringLength(1000)]
        [Display(Name = "Malfunction Description")]
        public string? MalfunctionDescription { get; set; }


        [StringLength(1000)]
        [Display(Name = "Work Performed")]
        public string? RepairWorkPerformed { get; set; }


        [StringLength(300)]
        [Display(Name = "Repair Result")]
        public string? RepairResult { get; set; }


        [StringLength(1000)]
        public string? Notes { get; set; }


        [Display(Name = "Workers")]
        public List<int> WorkerIds { get; set; }
            = new();
    }
}