using System.ComponentModel.DataAnnotations;

namespace CameraOperationsManagement.Models
{
    public class CameraVisit
    {
        public int Id { get; set; }


        [Required]
        [Display(Name = "Camera")]
        public int CameraId { get; set; }


        [Required]
        [Display(Name = "Visit Date")]
        public DateTime VisitDate { get; set; }


        [Required]
        [StringLength(300)]
        public string Purpose { get; set; } = string.Empty;


        // =========================
        // MALFUNCTION
        // =========================

        [StringLength(100)]
        [Display(Name = "Malfunction Type")]
        public string? MalfunctionType { get; set; }


        [StringLength(1000)]
        [Display(Name = "Malfunction Description")]
        public string? MalfunctionDescription { get; set; }


        // =========================
        // REPAIR
        // =========================

        [StringLength(1000)]
        [Display(Name = "Work Performed")]
        public string? RepairWorkPerformed { get; set; }


        [StringLength(300)]
        [Display(Name = "Repair Result")]
        public string? RepairResult { get; set; }


        // =========================
        // GENERAL
        // =========================

        [StringLength(1000)]
        public string? Notes { get; set; }


        public Camera Camera { get; set; } = null!;


        public ICollection<CameraVisitWorker> CameraVisitWorkers
        { get; set; } = new List<CameraVisitWorker>();
    }
}