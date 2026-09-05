namespace CameraOperationsManagement.ViewModels.Sites
{
    public class SiteDetailsViewModel
    {
        public string Id { get; set; }
            = string.Empty;

        public string Name { get; set; }
            = string.Empty;

        public string? Location { get; set; }

        public string? Notes { get; set; }

        public bool IsActive { get; set; }
    }
}