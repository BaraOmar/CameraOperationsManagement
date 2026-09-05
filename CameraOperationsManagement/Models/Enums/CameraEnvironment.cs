using System.ComponentModel.DataAnnotations;

namespace CameraOperationsManagement.Models.Enums
{
    public enum CameraEnvironment
    {
        [Display(Name = "Internal Camera")]
        Internal = 1,

        [Display(Name = "External Camera")]
        External = 2
    }
}