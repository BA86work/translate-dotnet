using Tesseract;
using RealTimeTranslator.Services.Interfaces;
using System.Drawing;
using System.Threading.Tasks;
using System.IO;
using System;

namespace RealTimeTranslator.Services.Implementations
{
    public class TesseractOcrService : IOcrService, IDisposable
    {
        private TesseractEngine _engine;
        private bool _isInitialized;
        private string _currentTessdataPath;
        private readonly string[] _supportedLanguages = { "eng", "jpn", "kor", "chi_sim", "tha" };

        public TesseractOcrService()
        {
            var tessdataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tessdata");
            if (!Directory.Exists(tessdataPath))
            {
                Directory.CreateDirectory(tessdataPath);
            }
            InitializeAsync(tessdataPath).GetAwaiter().GetResult();
        }

        public async Task InitializeAsync(string tessdataPath)
        {
            try 
            {
                if (_isInitialized && tessdataPath == _currentTessdataPath) return;

                _currentTessdataPath = tessdataPath;
                _engine?.Dispose();
                _engine = new TesseractEngine(tessdataPath, "eng", EngineMode.Default);
                _isInitialized = true;
                await Task.CompletedTask;
            }
            catch (Exception ex)
            {
                _isInitialized = false;
                throw new Exception($"Error initializing Tesseract OCR: {ex.Message}", ex);
            }
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

        public async Task<string> RecognizeTextAsync(Bitmap image)
        {
            try
            {
                if (_engine == null)
                {
                    throw new InvalidOperationException("OCR engine not initialized. Call InitializeAsync first.");
                }

                using var page = _engine.Process(image);
                return page.GetText().Trim();
            }
            catch (Exception ex)
            {
                throw new Exception($"OCR failed: {ex.Message}", ex);
            }
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