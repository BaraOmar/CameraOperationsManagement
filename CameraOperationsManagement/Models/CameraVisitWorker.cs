namespace CameraOperationsManagement.Models
{
    public class CameraVisitWorker
    {
        public int CameraVisitId { get; set; }

        public int WorkerId { get; set; }


        public CameraVisit CameraVisit { get; set; } = null!;

        public Worker Worker { get; set; } = null!;
    }
}