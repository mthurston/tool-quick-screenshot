using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Text;

namespace QuickScreenshot;

[SupportedOSPlatform("windows6.1")]
public class ScreenshotService
{
    // Windows API imports
    [DllImport("user32.dll")]
    private static extern IntPtr GetDesktopWindow();

    [DllImport("user32.dll")]
    private static extern IntPtr GetWindowDC(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern IntPtr CreateCompatibleBitmap(IntPtr hdc, int nWidth, int nHeight);

    [DllImport("gdi32.dll")]
    private static extern IntPtr SelectObject(IntPtr hdc, IntPtr hgdiobj);

    [DllImport("gdi32.dll")]
    private static extern bool BitBlt(IntPtr hdc, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hdcSrc, int nXSrc, int nYSrc, uint dwRop);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteDC(IntPtr hdc);

    [DllImport("gdi32.dll")]
    private static extern bool DeleteObject(IntPtr hObject);

    [DllImport("user32.dll")]
    private static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

    [DllImport("user32.dll")]
    private static extern bool EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);

    [DllImport("user32.dll")]
    private static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

    [DllImport("user32.dll")]
    private static extern int GetWindowTextLength(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern bool IsWindowVisible(IntPtr hWnd);

    [DllImport("user32.dll")]
    private static extern IntPtr FindWindow(string? lpClassName, string lpWindowName);

    [DllImport("user32.dll")]
    private static extern int GetSystemMetrics(int nIndex);

    // System metrics constants
    private const int SM_XVIRTUALSCREEN = 76;
    private const int SM_YVIRTUALSCREEN = 77;
    private const int SM_CXVIRTUALSCREEN = 78;
    private const int SM_CYVIRTUALSCREEN = 79;

    private const uint SRCCOPY = 0x00CC0020;

    public delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);

    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
    }

    public async Task TakeScreenshotAsync(string outputPath, ImageFormat format, int quality, string? windowTitle, int delay)
    {
        // Show a nice header
        Console.WriteLine("QuickScreenshot - Taking Screenshot");
        Console.WriteLine("===================================");

        if (delay > 0)
        {
            Console.WriteLine($"Waiting {delay} seconds before capturing...");
            
            for (int i = delay; i > 0; i--)
            {
                Console.Write($"\rTaking screenshot in {i} seconds...");
                await Task.Delay(1000);
            }
            Console.WriteLine("\rReady!                              ");
        }

        Bitmap? screenshot = null;

        try
        {
            if (!string.IsNullOrEmpty(windowTitle))
            {
                screenshot = await CaptureWindowAsync(windowTitle);
            }
            else
            {
                screenshot = CaptureFullScreen();
            }

            if (screenshot == null)
            {
                Console.WriteLine("Failed to capture screenshot");
                return;
            }

            // Save the screenshot
            await SaveScreenshotAsync(screenshot, outputPath, format, quality);
            
            var fileInfo = new FileInfo(outputPath);
            Console.WriteLine("✓ Screenshot saved successfully!");
            Console.WriteLine($"File Path: {outputPath}");
            Console.WriteLine($"Format: {format}");
            Console.WriteLine($"Size: {screenshot.Width}x{screenshot.Height}");
            Console.WriteLine($"File Size: {fileInfo.Length / 1024:N0} KB");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error: {ex.Message}");
            throw;
        }
        finally
        {
            screenshot?.Dispose();
        }
    }

    public Task ListWindowsAsync()
    {
        return Task.Run(() =>
        {
            Console.WriteLine("Available Windows");
            Console.WriteLine("=================");

            var windows = new List<(string Title, IntPtr Handle)>();

            EnumWindows((hWnd, lParam) =>
            {
                if (IsWindowVisible(hWnd))
                {
                    int length = GetWindowTextLength(hWnd);
                    if (length > 0)
                    {
                        var sb = new StringBuilder(length + 1);
                        GetWindowText(hWnd, sb, sb.Capacity);
                        var title = sb.ToString();
                        
                        if (!string.IsNullOrWhiteSpace(title))
                        {
                            windows.Add((title, hWnd));
                        }
                    }
                }
                return true;
            }, IntPtr.Zero);

            if (windows.Count == 0)
            {
                Console.WriteLine("No visible windows found.");
                return;
            }

            Console.WriteLine($"\n{"Window Title",-50} Handle");
            Console.WriteLine(new string('-', 70));

            foreach (var (title, handle) in windows.OrderBy(w => w.Title))
            {
                Console.WriteLine($"{title,-50} {handle}");
            }

            Console.WriteLine($"\nFound {windows.Count} visible windows");
        });
    }

    private Bitmap CaptureFullScreen()
    {
        Console.WriteLine("Capturing full screen...");
        
        // Get virtual screen bounds using Windows API
        var left = GetSystemMetrics(SM_XVIRTUALSCREEN);
        var top = GetSystemMetrics(SM_YVIRTUALSCREEN);
        var width = GetSystemMetrics(SM_CXVIRTUALSCREEN);
        var height = GetSystemMetrics(SM_CYVIRTUALSCREEN);
        
        var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(left, top, 0, 0, new Size(width, height));
        
        return bitmap;
    }

    private Task<Bitmap?> CaptureWindowAsync(string windowTitle)
    {
        return Task.Run(() =>
        {
        Console.WriteLine($"Searching for window: '{windowTitle}'...");
        
        IntPtr targetWindow = IntPtr.Zero;
        string? foundTitle = null;

        EnumWindows((hWnd, lParam) =>
        {
            if (IsWindowVisible(hWnd))
            {
                int length = GetWindowTextLength(hWnd);
                if (length > 0)
                {
                    var sb = new StringBuilder(length + 1);
                    GetWindowText(hWnd, sb, sb.Capacity);
                    var title = sb.ToString();
                    
                    if (title.Contains(windowTitle, StringComparison.OrdinalIgnoreCase))
                    {
                        targetWindow = hWnd;
                        foundTitle = title;
                        return false; // Stop enumeration
                    }
                }
            }
            return true;
        }, IntPtr.Zero);

        if (targetWindow == IntPtr.Zero)
        {
            Console.WriteLine($"Window containing '{windowTitle}' not found.");
            return null;
        }

        Console.WriteLine($"Found window: '{foundTitle}'");

        if (!GetWindowRect(targetWindow, out RECT rect))
        {
            Console.WriteLine("Failed to get window rectangle.");
            return null;
        }

        int width = rect.Right - rect.Left;
        int height = rect.Bottom - rect.Top;

        if (width <= 0 || height <= 0)
        {
            Console.WriteLine("Invalid window dimensions.");
            return null;
        }

        var bitmap = new Bitmap(width, height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, new System.Drawing.Size(width, height));
        
        return bitmap;
        });
    }



    private Task SaveScreenshotAsync(Bitmap screenshot, string outputPath, ImageFormat format, int quality)
    {
        return Task.Run(() =>
        {
        Console.WriteLine("Saving screenshot...");
        
        // Ensure directory exists
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Adjust file extension based on format
        var extension = format switch
        {
            var f when f.Equals(ImageFormat.Png) => ".png",
            var f when f.Equals(ImageFormat.Jpeg) => ".jpg",
            var f when f.Equals(ImageFormat.Bmp) => ".bmp",
            var f when f.Equals(ImageFormat.Gif) => ".gif",
            _ => ".png"
        };

        if (!outputPath.EndsWith(extension, StringComparison.OrdinalIgnoreCase))
        {
            outputPath = Path.ChangeExtension(outputPath, extension);
        }

        if (format.Equals(ImageFormat.Jpeg))
        {
            // For JPEG, we need to set quality
            var encoderParameters = new EncoderParameters(1);
            encoderParameters.Param[0] = new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, (long)quality);
            
            var jpegCodec = ImageCodecInfo.GetImageDecoders()
                .First(codec => codec.FormatID == ImageFormat.Jpeg.Guid);
            
            screenshot.Save(outputPath, jpegCodec, encoderParameters);
        }
        else
        {
            screenshot.Save(outputPath, format);
        }
        });
    }
}
