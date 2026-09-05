using System.ComponentModel.DataAnnotations;

namespace CameraOperationsManagement.ViewModels.Users
{
    public class EditUserViewModel
    {
        public string Id { get; set; }
            = string.Empty;


        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }
            = string.Empty;


        [Required]
        [Display(Name = "Second Name")]
        public string SecondName { get; set; }
            = string.Empty;


        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }
            = string.Empty;


        [Required]
        [EmailAddress]
        public string Email { get; set; }
            = string.Empty;


        [Required(ErrorMessage = "Please select a role.")]
        public string Role { get; set; }
            = string.Empty;
    }
}