using System.CommandLine;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace QuickScreenshot;

public enum SupportedFormat
{
    PNG,
    JPEG,
    BMP,
    GIF
}

class Program
{
    [SupportedOSPlatform("windows6.1")]
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("QuickScreenshot - A fast CLI tool for taking screenshots");

        // Options
        var outputOption = new Option<string>("--output", "-o")
        {
            Description = "Output file path (default: screenshots/screenshot_[timestamp].png)"
        };
        outputOption.DefaultValueFactory = _ => $"screenshots/screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";

        var formatOption = new Option<SupportedFormat>("--format", "-f")
        {
            Description = "Image format (PNG, JPEG, BMP, GIF)"
        };
        formatOption.DefaultValueFactory = _ => SupportedFormat.PNG;

        var qualityOption = new Option<int>("--quality", "-q")
        {
            Description = "JPEG quality (1-100, only applies to JPEG format)"
        };
        qualityOption.DefaultValueFactory = _ => 90;

        var windowOption = new Option<string?>("--window", "-w")
        {
            Description = "Capture a specific window by title (partial match)"
        };

        var listWindowsOption = new Option<bool>("--list-windows", "-l")
        {
            Description = "List all available windows"
        };

        var delayOption = new Option<int>("--delay", "-d")
        {
            Description = "Delay in seconds before taking screenshot"
        };
        delayOption.DefaultValueFactory = _ => 0;

        // Add options to root command
        rootCommand.Options.Add(outputOption);
        rootCommand.Options.Add(formatOption);
        rootCommand.Options.Add(qualityOption);
        rootCommand.Options.Add(windowOption);
        rootCommand.Options.Add(listWindowsOption);
        rootCommand.Options.Add(delayOption);

        rootCommand.SetAction(async (parseResult, cancellationToken) =>
        {
            var output = parseResult.GetValue(outputOption);
            var format = parseResult.GetValue(formatOption);
            var quality = parseResult.GetValue(qualityOption);
            var window = parseResult.GetValue(windowOption);
            var listWindows = parseResult.GetValue(listWindowsOption);
            var delay = parseResult.GetValue(delayOption);

            var screenshotService = new ScreenshotService();
            
            try
            {
                if (listWindows)
                {
                    await screenshotService.ListWindowsAsync();
                    return 0;
                }

                // Convert SupportedFormat to ImageFormat
                var imageFormat = format switch
                {
                    SupportedFormat.PNG => ImageFormat.Png,
                    SupportedFormat.JPEG => ImageFormat.Jpeg,
                    SupportedFormat.BMP => ImageFormat.Bmp,
                    SupportedFormat.GIF => ImageFormat.Gif,
                    _ => ImageFormat.Png
                };

                await screenshotService.TakeScreenshotAsync(output!, imageFormat, quality, window, delay);
                return 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                return 1;
            }
        });

        var parseResult = rootCommand.Parse(args);
        return await parseResult.InvokeAsync();
    }
}
