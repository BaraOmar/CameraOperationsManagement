namespace CameraOperationsManagement.ViewModels.Sites
{
    public class SiteStructureViewModel
    {
        public string SiteId { get; set; } = string.Empty;

        public string SiteName { get; set; } = string.Empty;

        public string? Location { get; set; }

        public bool IsActive { get; set; }

        public List<SiteStructureRecorderViewModel> Recorders
        { get; set; } = new();

        public List<SiteStructureSwitchViewModel> Switches
        { get; set; } = new();
    }


    public class SiteStructureRecorderViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public string? NetworkSwitchName { get; set; }

        public List<SiteStructureCameraViewModel> Cameras
        { get; set; } = new();
    }


    public class SiteStructureCameraViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public string? Type { get; set; }

        public string? IpAddress { get; set; }

        public string InstallationLocation { get; set; }
            = string.Empty;

        public bool IsActive { get; set; }
    }


    public class SiteStructureSwitchViewModel
    {
        public int Id { get; set; }

        public string Name { get; set; } = string.Empty;

        public bool IsActive { get; set; }
    }
}