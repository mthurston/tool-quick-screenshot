# QuickScreenshot

[![CI](https://github.com/mthurston/tool-quick-screenshot/actions/workflows/ci.yml/badge.svg)](https://github.com/mthurston/tool-quick-screenshot/actions/workflows/ci.yml)

A fast, lightweight CLI tool for taking screenshots on Windows with a clean text-based interface.

## Features

- 🚀 **Fast & Lightweight**: Ready for AOT compilation for instant startup
- 🎨 **Clean CLI**: Simple, efficient text-based interface
- 📸 **Multiple Capture Modes**: Full screen and specific windows
- 🖼️ **Format Support**: PNG, JPEG, BMP, GIF
- ⚡ **Latest Microsoft CLI Library**: Built with System.CommandLine
- 🎯 **Window Detection**: Smart window finding with partial title matching
- ⏱️ **Delayed Capture**: Built-in countdown timer
- 🔧 **Quality Control**: Adjustable JPEG quality settings
- 🪟 **Pure Windows API**: No Windows Forms dependency for better AOT compatibility

## Installation

### Install as a global .NET tool:

```powershell
dotnet pack QuickScreenshot.csproj -c Release --output ./nupkg
dotnet tool install --global --add-source ./nupkg QuickScreenshot
```

### Build from source:

```powershell
git clone https://github.com/mthurston/tool-quick-screenshot.git
cd tool-quick-screenshot
dotnet build
```

### Create AOT-compiled executable:

**Note**: AOT compilation requires Visual Studio with "Desktop Development with C++" workload installed.

```powershell
dotnet publish -c Release -r win-x64 --self-contained -o ./publish -p:IsPublishing=true
```

### Create regular self-contained executable:

```powershell
dotnet publish -c Release -r win-x64 --self-contained -o ./publish -p:PublishAot=false
```

## Usage

**Note**: The examples below show usage as an installed global tool (`qscreenshot`). When running from source during development, replace `qscreenshot` with `dotnet run --`.

### Basic screenshot (full screen):
```bash
qscreenshot
```

### Save to specific location:
```bash
qscreenshot -o "C:\Screenshots\my-screenshot.png"
```

### Capture specific window:
```bash
qscreenshot --window "Visual Studio Code"
```

### List all windows:
```bash
qscreenshot --list-windows
```

### Delayed capture with countdown:
```bash
qscreenshot --delay 5
```

### Different formats and quality:
```bash
qscreenshot --format JPEG --quality 95 -o screenshot.jpg
```

## Command Line Options

| Option | Short | Description | Default |
|--------|-------|-------------|---------|
| `--output` | `-o` | Output file path | `screenshots/screenshot_[timestamp].png` |
| `--format` | `-f` | Image format (PNG, JPEG, BMP, GIF) | `PNG` |
| `--quality` | `-q` | JPEG quality (1-100) | `90` |
| `--window` | `-w` | Capture specific window by title | - |
| `--list-windows` | `-l` | List all available windows | `false` |
| `--delay` | `-d` | Delay in seconds before capture | `0` |

## Examples

### Capture Visual Studio Code window:
```bash
qscreenshot -w "Code" -o vscode-screenshot.png
```

### High-quality JPEG with 3-second delay:
```bash
qscreenshot -f JPEG -q 98 -d 3 -o high-quality.jpg
```

### Quick window list and capture:
```bash
# First, see available windows
qscreenshot -l

# Then capture the one you want
qscreenshot -w "Chrome" -o browser.png
```

## Requirements

- Windows 10 version 1809 (10.0.17763) or later
- .NET 8.0 runtime (for regular build) or no dependencies (for AOT build)

## Building

This project targets `net8.0` and uses:
- **System.CommandLine** for modern CLI parsing
- **System.Drawing.Common** for image manipulation
- **Windows API** (P/Invoke) for screen capture functionality
- **AOT compilation** support for fast startup and deployment

```powershell
# Debug build
dotnet build QuickScreenshot.csproj

# Release build
dotnet build QuickScreenshot.csproj -c Release

# AOT publish (requires Visual Studio C++ tools)
dotnet publish -c Release -r win-x64 --self-contained -o ./publish -p:IsPublishing=true

# Regular self-contained publish
dotnet publish -c Release -r win-x64 --self-contained -o ./publish -p:PublishAot=false

# Create NuGet package
dotnet pack QuickScreenshot.csproj -c Release --output ./nupkg
```

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

### Development Setup
1. Clone the repository
2. Install .NET 8.0 SDK
3. Run `dotnet restore QuickScreenshot.csproj` to install dependencies
4. Use `dotnet run -- --help` to test during development

### Automated Quality Assurance
This project includes:
- **GitHub Actions CI**: Automated building and testing on push/PR to main and develop branches
- **Dependabot**: Automatic dependency updates and vulnerability scanning for NuGet packages and GitHub Actions

## License

This project is licensed under the terms specified in the LICENSE file.
