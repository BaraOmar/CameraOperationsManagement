using System.ComponentModel.DataAnnotations;

namespace CameraOperationsManagement.Models
{
    public class SiteVisit
    {
        public int Id { get; set; }


        [Required]
        [StringLength(50)]
        [Display(Name = "Site")]
        public string SiteId { get; set; } = string.Empty;


        [Required]
        [Display(Name = "Visit Date")]
        public DateTime VisitDate { get; set; }


        [Required]
        [StringLength(300)]
        public string Purpose { get; set; } = string.Empty;


        [StringLength(1000)]
        public string? Notes { get; set; }


        public Site Site { get; set; } = null!;


        public ICollection<SiteVisitWorker> SiteVisitWorkers
        { get; set; } = new List<SiteVisitWorker>();
    }
}