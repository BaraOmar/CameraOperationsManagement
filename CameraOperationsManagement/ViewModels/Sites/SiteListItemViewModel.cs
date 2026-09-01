namespace CameraOperationsManagement.ViewModels.Sites
{
    public class SiteListItemViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string? Location { get; set; }

        public bool IsActive { get; set; }
    }
}