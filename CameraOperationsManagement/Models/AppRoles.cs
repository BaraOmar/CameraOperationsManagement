namespace CameraOperationsManagement.Models
{
    public static class AppRoles
    {
        public const string Admin =
            "Admin";

        public const string InfrastructureManager =
            "InfrastructureManager";

        public const string Editor =
            "Editor";

        public const string InfrastructureViewer =
            "InfrastructureViewer";

        public const string Viewer =
            "Viewer";


        public static readonly string[] All =
        {
            Admin,
            InfrastructureManager,
            Editor,
            InfrastructureViewer,
            Viewer
        };
    }
}