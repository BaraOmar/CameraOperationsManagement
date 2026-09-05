using CameraOperationsManagement.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace CameraOperationsManagement.Models
{
    public class Visit
    {
        public int Id { get; set; }


        // SITE

        [Required]
        [StringLength(50)]
        [Display(Name = "Site")]
        public string SiteId { get; set; }
            = string.Empty;


        // COMPONENT

        [Required]
        [Display(Name = "Component Type")]
        public VisitComponentType ComponentType
        { get; set; }


        public int? RecorderId { get; set; }

        public int? NetworkSwitchId { get; set; }

        public int? CameraId { get; set; }


        // VISIT INFORMATION

        [Required]
        [Display(Name = "Visit Date")]
        public DateTime VisitDate { get; set; }


        [Required]
        [StringLength(300)]
        public string Purpose { get; set; }
            = string.Empty;


        // MALFUNCTION

        [StringLength(100)]
        [Display(Name = "Malfunction Type")]
        public string? MalfunctionType { get; set; }


        [StringLength(1000)]
        [Display(Name = "Malfunction Description")]
        public string? MalfunctionDescription { get; set; }


        // REPAIR

        [StringLength(1000)]
        [Display(Name = "Work Performed")]
        public string? RepairWorkPerformed { get; set; }


        [StringLength(300)]
        [Display(Name = "Repair Result")]
        public string? RepairResult { get; set; }


        // GENERAL

        [StringLength(1000)]
        public string? Notes { get; set; }


        // NAVIGATION

        public Site Site { get; set; } = null!;

        public Recorder? Recorder { get; set; }

        public NetworkSwitch? NetworkSwitch { get; set; }

        public Camera? Camera { get; set; }


        public ICollection<VisitWorker> VisitWorkers
        { get; set; } = new List<VisitWorker>();
    }
}