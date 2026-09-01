using System.ComponentModel.DataAnnotations;

namespace CameraOperationsManagement.ViewModels.Workers
{
    public class WorkerFormViewModel
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Second Name")]
        public string SecondName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; } = string.Empty;
    }
}