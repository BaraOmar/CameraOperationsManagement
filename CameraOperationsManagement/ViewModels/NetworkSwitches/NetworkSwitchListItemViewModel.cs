namespace CameraOperationsManagement.ViewModels.NetworkSwitches
{
    public class NetworkSwitchListItemViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string SiteId { get; set; } = string.Empty;

        public string SiteName { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}