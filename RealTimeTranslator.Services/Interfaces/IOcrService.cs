using System.Drawing;
using System.Threading.Tasks;

namespace RealTimeTranslator.Services.Interfaces
{
    public interface IOcrService
    {
        Task<string> ExtractTextFromImageAsync(Bitmap image);
        Task InitializeAsync(string tessdataPath);
    }
} 