using RealTimeTranslator.Services.Interfaces;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using Windows.Graphics.Capture;
using Windows.Graphics;
using Windows.Graphics.DirectX;
using Windows.Graphics.DirectX.Direct3D11;
using SharpDX.Direct3D11;
using SharpDX.DXGI;
using Device = SharpDX.Direct3D11.Device;
using System.Threading.Tasks;
using WinRT;

namespace RealTimeTranslator.Services.Implementations
{
    public class WindowsScreenCaptureService : IScreenCaptureService, IDisposable
    {
        private Rectangle _captureArea;
        private bool _isCapturing;
        private GraphicsCaptureSession? _captureSession;
        private Direct3D11CaptureFramePool? _framePool;
        private Device? _device;
        private SwapChain1? _swapChain;
        private IDirect3DDevice? _d3dDevice;

        public void SetCaptureArea(Rectangle area)
        {
            _captureArea = area;
        }

        public async Task<Bitmap> CaptureAreaAsync(Rectangle area)
        {
            var bitmap = new Bitmap(area.Width, area.Height, PixelFormat.Format32bppArgb);
            await Task.Run(() =>
            {
                using var graphics = Graphics.FromImage(bitmap);
                graphics.CopyFromScreen(area.Left, area.Top, 0, 0, area.Size);
            });
            return bitmap;
        }

        public async Task StartCaptureAsync()
        {
            if (_isCapturing) return;

            var picker = new GraphicsCapturePicker();
            var item = await picker.PickSingleItemAsync();

            if (item != null)
            {
                // Create D3D11 device
                _device = new Device(SharpDX.Direct3D.DriverType.Hardware, DeviceCreationFlags.BgraSupport);
                
                // Create D3D device for Windows.Graphics.Capture
                var dxgiDevice = _device.QueryInterface<SharpDX.DXGI.Device>();
                var d3dDevice = CreateD3DDeviceFromSharpDX(dxgiDevice);
                _d3dDevice = d3dDevice;

                // Create swap chain
                var swapChainDesc = new SwapChainDescription1
                {
                    Width = item.Size.Width,
                    Height = item.Size.Height,
                    Format = Format.B8G8R8A8_UNorm,
                    Stereo = false,
                    SampleDescription = new SampleDescription(1, 0),
                    Usage = Usage.RenderTargetOutput,
                    BufferCount = 2,
                    Scaling = Scaling.Stretch,
                    SwapEffect = SwapEffect.FlipSequential,
                    AlphaMode = AlphaMode.Premultiplied,
                    Flags = SwapChainFlags.None
                };

                using (var factory = new Factory2())
                {
                    _swapChain = new SwapChain1(factory, _device, ref swapChainDesc);
                }

                // Create frame pool
                _framePool = Direct3D11CaptureFramePool.Create(
                    _d3dDevice,
                    DirectXPixelFormat.B8G8R8A8UIntNormalized,
                    2,
                    item.Size);

                _captureSession = _framePool.CreateCaptureSession(item);
                _framePool.FrameArrived += FramePool_FrameArrived;
                _captureSession.StartCapture();
                _isCapturing = true;
            }
        }

        private IDirect3DDevice CreateD3DDeviceFromSharpDX(SharpDX.DXGI.Device dxgiDevice)
        {
            var access = Marshal.GetIUnknownForObject(dxgiDevice);
            var d3dDevice = MarshalInterface<IDirect3DDevice>.FromAbi(access);
            Marshal.Release(access);
            return d3dDevice;
        }

        public Task StopCaptureAsync()
        {
            if (!_isCapturing) return Task.CompletedTask;

            _captureSession?.Dispose();
            _framePool?.Dispose();
            _swapChain?.Dispose();
            _device?.Dispose();
            _d3dDevice = null;
            _isCapturing = false;

            return Task.CompletedTask;
        }

        private void FramePool_FrameArrived(Direct3D11CaptureFramePool sender, object args)
        {
            using var frame = sender.TryGetNextFrame();
            if (frame != null)
            {
                var frameTexture = Direct3D11Helper.CreateSharpDXTexture2D(frame.Surface);
                if (frameTexture != null)
                {
                    // Copy frame to swap chain back buffer
                    using var backBuffer = _swapChain?.GetBackBuffer<Texture2D>(0);
                    if (backBuffer != null)
                    {
                        _device?.ImmediateContext.CopyResource(frameTexture, backBuffer);
                        _swapChain?.Present(1, PresentFlags.None);
                    }
                    frameTexture.Dispose();
                }
            }
        }

        public void Dispose()
        {
            StopCaptureAsync().Wait();
            _captureSession?.Dispose();
            _framePool?.Dispose();
            _swapChain?.Dispose();
            _device?.Dispose();
            GC.SuppressFinalize(this);
        }
    }

    internal static class Direct3D11Helper
    {
        public static Texture2D? CreateSharpDXTexture2D(IDirect3DSurface surface)
        {
            try
            {
                var access = Marshal.GetIUnknownForObject(surface);
                var d3dTexture = new Texture2D(access);
                Marshal.Release(access);
                return d3dTexture;
            }
            catch
            {
                return null;
            }
        }
    }
} 