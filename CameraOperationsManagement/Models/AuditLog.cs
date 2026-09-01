using System.ComponentModel.DataAnnotations;

namespace CameraOperationsManagement.Models
{
    public class AuditLog
    {
        public int Id { get; set; }


        [Required]
        [StringLength(450)]
        public string UserId { get; set; } = string.Empty;


        [Required]
        [StringLength(200)]
        [Display(Name = "Performed By")]
        public string UserDisplayName { get; set; } = string.Empty;


        [StringLength(256)]
        public string? UserEmail { get; set; }


        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty;


        [Required]
        [StringLength(100)]
        [Display(Name = "Entity")]
        public string EntityType { get; set; } = string.Empty;


        [Required]
        [StringLength(100)]
        [Display(Name = "Entity ID")]
        public string EntityId { get; set; } = string.Empty;


        [StringLength(1000)]
        public string? Description { get; set; }


        [Required]
        [Display(Name = "Date")]
        public DateTime PerformedAtUtc { get; set; }
            = DateTime.UtcNow;
    }
}