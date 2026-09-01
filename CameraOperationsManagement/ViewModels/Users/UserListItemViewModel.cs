namespace CameraOperationsManagement.ViewModels.Users
{
    public class UserListItemViewModel
    {
        public string Id { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;

        public string SecondName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public string FullName =>
            $"{FirstName} {SecondName} {LastName}";
    }
}