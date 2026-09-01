using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CameraOperationsManagement.Models
{
    public class Worker
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


        public bool IsActive { get; set; } = true;


        [NotMapped]
        public string FullName =>
            $"{FirstName} {SecondName} {LastName}";
    }
}