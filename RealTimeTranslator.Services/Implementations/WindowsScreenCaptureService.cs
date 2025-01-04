using RealTimeTranslator.Services.Interfaces;
using System;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;
using System.Drawing.Imaging;
using System.IO;

namespace RealTimeTranslator.Services.Implementations
{
    public class WindowsScreenCaptureService : IScreenCaptureService
    {
        private readonly IOcrService _ocrService;
        private Rectangle _captureArea;
        private Form? _selectionForm;

        public WindowsScreenCaptureService(IOcrService ocrService)
        {
            _ocrService = ocrService;
        }

        public void SetCaptureArea(Rectangle area)
        {
            _captureArea = area;
        }

        public async Task<Bitmap> CaptureAreaAsync(Rectangle area)
        {
            try
            {
                var bitmap = new Bitmap(area.Width, area.Height);
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(area.X, area.Y, 0, 0, area.Size);
                }
                return bitmap;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to capture screen area: {ex.Message}", ex);
            }
        }

        public async Task<string> CaptureAndRecognizeTextAsync()
        {
            try
            {
                var tcs = new TaskCompletionSource<Rectangle>();
                
                await Task.Run(() =>
                {
                    _selectionForm = new Form
                    {
                        WindowState = FormWindowState.Maximized,
                        FormBorderStyle = FormBorderStyle.None,
                        Opacity = 0.5,
                        BackColor = Color.Black,
                        Cursor = Cursors.Cross,
                        TopMost = true
                    };

                    Point startPoint = Point.Empty;
                    bool isMouseDown = false;
                    Rectangle selectedArea = Rectangle.Empty;

                    _selectionForm.MouseDown += (s, e) =>
                    {
                        if (e.Button == MouseButtons.Left)
                        {
                            startPoint = e.Location;
                            isMouseDown = true;
                            selectedArea = Rectangle.Empty;
                        }
                    };

                    _selectionForm.MouseMove += (s, e) =>
                    {
                        if (isMouseDown)
                        {
                            int x = Math.Min(startPoint.X, e.X);
                            int y = Math.Min(startPoint.Y, e.Y);
                            int width = Math.Abs(e.X - startPoint.X);
                            int height = Math.Abs(e.Y - startPoint.Y);
                            selectedArea = new Rectangle(x, y, width, height);
                            _selectionForm.Invalidate();
                        }
                    };

                    _selectionForm.MouseUp += (s, e) =>
                    {
                        if (e.Button == MouseButtons.Left)
                        {
                            isMouseDown = false;
                            _selectionForm.Hide();
                            tcs.SetResult(selectedArea);
                        }
                    };

                    _selectionForm.Paint += (s, e) =>
                    {
                        if (selectedArea != Rectangle.Empty)
                        {
                            using (Pen pen = new Pen(Color.Red, 2))
                            {
                                e.Graphics.DrawRectangle(pen, selectedArea);
                            }
                        }
                    };

                    _selectionForm.KeyDown += (s, e) =>
                    {
                        if (e.KeyCode == Keys.Escape)
                        {
                            _selectionForm.Hide();
                            tcs.SetResult(Rectangle.Empty);
                        }
                    };

                    _selectionForm.ShowDialog();
                });

                var area = await tcs.Task;
                if (area == Rectangle.Empty)
                {
                    return string.Empty;
                }

                using var bitmap = await CaptureAreaAsync(area);
                return await _ocrService.RecognizeTextAsync(bitmap);
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to capture and recognize text: {ex.Message}", ex);
            }
            finally
            {
                if (_selectionForm != null)
                {
                    _selectionForm.Dispose();
                    _selectionForm = null;
                }
            }
        }

        public Task StartCaptureAsync()
        {
            return Task.CompletedTask;
        }

        public Task StopCaptureAsync()
        {
            return Task.CompletedTask;
        }
    }
} 