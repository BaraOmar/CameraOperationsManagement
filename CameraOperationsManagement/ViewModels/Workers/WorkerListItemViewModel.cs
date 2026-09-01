namespace CameraOperationsManagement.ViewModels.Workers
{
    public class WorkerListItemViewModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = string.Empty;

        public string SecondName { get; set; } = string.Empty;

        public string LastName { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public string FullName =>
            $"{FirstName} {SecondName} {LastName}";
    }
}