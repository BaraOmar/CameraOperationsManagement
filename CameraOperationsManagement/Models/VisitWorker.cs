namespace CameraOperationsManagement.Models
{
    public class VisitWorker
    {
        public int VisitId { get; set; }

        public int WorkerId { get; set; }


        public Visit Visit { get; set; }
            = null!;

        public Worker Worker { get; set; }
            = null!;
    }
}