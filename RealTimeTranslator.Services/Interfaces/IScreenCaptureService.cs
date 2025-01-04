using System.Drawing;
using System.Threading.Tasks;

namespace RealTimeTranslator.Services.Interfaces
{
    public interface IScreenCaptureService
    {
        Task<Bitmap> CaptureAreaAsync(Rectangle area);
        void SetCaptureArea(Rectangle area);
        Task StartCaptureAsync();
        Task StopCaptureAsync();
    }
} 