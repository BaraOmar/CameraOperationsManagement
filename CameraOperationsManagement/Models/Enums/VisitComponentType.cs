using System.ComponentModel.DataAnnotations;

namespace CameraOperationsManagement.Models.Enums
{
    public enum VisitComponentType
    {
        [Display(Name = "Recorder")]
        Recorder = 1,

        [Display(Name = "Switch")]
        Switch = 2,

        [Display(Name = "Camera")]
        Camera = 3

        // Monitor = 4 later
    }
}