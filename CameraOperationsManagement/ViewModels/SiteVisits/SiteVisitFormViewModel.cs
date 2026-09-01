using System.ComponentModel.DataAnnotations;

namespace CameraOperationsManagement.ViewModels.SiteVisits
{
    public class SiteVisitFormViewModel
    {
        public int Id { get; set; }


        [Required]
        [Display(Name = "Site")]
        public string SiteId { get; set; } = string.Empty;


        [Required]
        [Display(Name = "Visit Date")]
        [DataType(DataType.DateTime)]
        public DateTime VisitDate { get; set; } = DateTime.Now;


        [Required]
        [StringLength(300)]
        public string Purpose { get; set; } = string.Empty;


        [StringLength(1000)]
        public string? Notes { get; set; }


        [Display(Name = "Workers")]
        public List<int> WorkerIds { get; set; } = new();
    }
}