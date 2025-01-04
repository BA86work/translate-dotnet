using Tesseract;
using RealTimeTranslator.Services.Interfaces;
using System.Drawing;
using System.Threading.Tasks;
using System.IO;

namespace RealTimeTranslator.Services.Implementations
{
    public class TesseractOcrService : IOcrService, IDisposable
    {
        private TesseractEngine _engine;
        private bool _isInitialized;
        private string _currentTessdataPath;
        private readonly string[] _supportedLanguages = { "eng", "jpn", "kor", "chi_sim", "tha" };

        public async Task InitializeAsync(string tessdataPath)
        {
            if (_isInitialized && tessdataPath == _currentTessdataPath) return;

            await Task.Run(() =>
            {
                if (!Directory.Exists(tessdataPath))
                {
                    throw new DirectoryNotFoundException($"Tessdata directory not found: {tessdataPath}");
                }

                _engine?.Dispose();
                _engine = new TesseractEngine(tessdataPath, "eng", EngineMode.Default);
                _currentTessdataPath = tessdataPath;
                _isInitialized = true;
            });
        }

        public async Task<string> ExtractTextFromImageAsync(Bitmap image)
        {
            if (!_isInitialized)
                throw new InvalidOperationException("OCR service is not initialized");

            return await Task.Run(() =>
            {
                using var pix = Pix.LoadFromMemory(ImageToByte(image));
                using var page = _engine.Process(pix);
                return page.GetText().Trim();
            });
        }

        private byte[] ImageToByte(Bitmap image)
        {
            using var stream = new MemoryStream();
            image.Save(stream, System.Drawing.Imaging.ImageFormat.Png);
            return stream.ToArray();
        }

        public void SetLanguage(string language)
        {
            if (!_supportedLanguages.Contains(language))
                throw new ArgumentException($"Unsupported language: {language}");

            if (!_isInitialized)
                throw new InvalidOperationException("OCR service is not initialized");

            var newEngine = new TesseractEngine(_currentTessdataPath, language, EngineMode.Default);
            var oldEngine = _engine;
            _engine = newEngine;
            oldEngine?.Dispose();
        }

        public void Dispose()
        {
            _engine?.Dispose();
            GC.SuppressFinalize(this);
        }
    }
} 