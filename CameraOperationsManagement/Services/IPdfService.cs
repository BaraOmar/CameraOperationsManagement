using CameraOperationsManagement.ViewModels.Cameras;
using CameraOperationsManagement.ViewModels.Sites;

namespace CameraOperationsManagement.Services
{
    public interface IPdfService
    {
        byte[] GenerateCameraHistoryPdf(
            CameraHistoryViewModel model);

        byte[] GenerateSiteHistoryPdf(
            SiteHistoryViewModel model);

        byte[] GenerateCameraListPdf(
    IEnumerable<CameraListItemViewModel> cameras);

        byte[] GenerateSiteStructurePdf(
    SiteStructureViewModel model);

    }
}