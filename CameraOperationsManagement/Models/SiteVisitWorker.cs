namespace CameraOperationsManagement.Models
{
    public class SiteVisitWorker
    {
        public int SiteVisitId { get; set; }

        public int WorkerId { get; set; }


        public SiteVisit SiteVisit { get; set; } = null!;

        public Worker Worker { get; set; } = null!;
    }
}