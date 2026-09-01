using CameraOperationsManagement.ViewModels.Cameras;

namespace CameraOperationsManagement.Services
{
    public interface IPdfService
    {
        byte[] GenerateCameraHistoryPdf(
            CameraHistoryViewModel model);
    }
}