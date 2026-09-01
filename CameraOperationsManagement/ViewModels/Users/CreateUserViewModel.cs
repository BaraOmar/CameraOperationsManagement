using System.ComponentModel.DataAnnotations;

namespace CameraOperationsManagement.ViewModels.Users
{
    public class CreateUserViewModel
    {
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


        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;


        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;


        [Required]
        public string Role { get; set; } = string.Empty;
    }
}