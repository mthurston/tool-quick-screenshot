using System.CommandLine;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

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
    static async Task<int> Main(string[] args)
    {
        var rootCommand = new RootCommand("QuickScreenshot - A fast CLI tool for taking screenshots");

        // Options
        var outputOption = new Option<string>(
            aliases: ["--output", "-o"],
            description: "Output file path (default: screenshots/screenshot_[timestamp].png)",
            getDefaultValue: () => $"screenshots/screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png"
        );

        var formatOption = new Option<SupportedFormat>(
            aliases: ["--format", "-f"],
            description: "Image format (PNG, JPEG, BMP, GIF)",
            getDefaultValue: () => SupportedFormat.PNG
        );

        var qualityOption = new Option<int>(
            aliases: ["--quality", "-q"],
            description: "JPEG quality (1-100, only applies to JPEG format)",
            getDefaultValue: () => 90
        );



        var windowOption = new Option<string?>(
            aliases: ["--window", "-w"],
            description: "Capture a specific window by title (partial match)"
        );

        var listWindowsOption = new Option<bool>(
            aliases: ["--list-windows", "-l"],
            description: "List all available windows"
        );

        var delayOption = new Option<int>(
            aliases: ["--delay", "-d"],
            description: "Delay in seconds before taking screenshot",
            getDefaultValue: () => 0
        );

        // Add options to root command
        rootCommand.AddOption(outputOption);
        rootCommand.AddOption(formatOption);
        rootCommand.AddOption(qualityOption);
        rootCommand.AddOption(windowOption);
        rootCommand.AddOption(listWindowsOption);
        rootCommand.AddOption(delayOption);

        rootCommand.SetHandler(async (string output, SupportedFormat format, int quality, string? window, bool listWindows, int delay) =>
        {
            var screenshotService = new ScreenshotService();
            
            try
            {
                if (listWindows)
                {
                    await screenshotService.ListWindowsAsync();
                    return;
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

                await screenshotService.TakeScreenshotAsync(output, imageFormat, quality, window, delay);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine(ex.StackTrace);
                Environment.Exit(1);
            }
        }, outputOption, formatOption, qualityOption, windowOption, listWindowsOption, delayOption);

        return await rootCommand.InvokeAsync(args);
    }
}
