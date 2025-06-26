#!/usr/bin/env pwsh

Write-Host "Building QuickScreenshot..." -ForegroundColor Green

Write-Host "`n=== Cleaning previous builds ===" -ForegroundColor Yellow
dotnet clean QuickScreenshot.csproj

Write-Host "`n=== Restoring packages ===" -ForegroundColor Yellow  
dotnet restore QuickScreenshot.csproj

Write-Host "`n=== Building Debug ===" -ForegroundColor Yellow
dotnet build QuickScreenshot.csproj -c Debug

Write-Host "`n=== Building Release ===" -ForegroundColor Yellow
dotnet build QuickScreenshot.csproj -c Release

Write-Host "`n=== Running tests (if any) ===" -ForegroundColor Yellow
dotnet test QuickScreenshot.csproj --no-build -c Release

Write-Host "`n=== Creating self-contained executable ===" -ForegroundColor Yellow
dotnet publish QuickScreenshot.csproj -c Release -r win-x64 --self-contained -o ./publish -p:PublishAot=false

Write-Host "`n=== Attempting AOT compilation (requires Visual Studio C++ tools) ===" -ForegroundColor Yellow
try {
    dotnet publish QuickScreenshot.csproj -c Release -r win-x64 --self-contained -o ./publish-aot -p:IsPublishing=true 2>$null
    Write-Host "AOT compilation successful!" -ForegroundColor Green
} catch {
    Write-Host "AOT compilation failed - requires Visual Studio C++ workload" -ForegroundColor Yellow
}

Write-Host "`n=== Creating NuGet package ===" -ForegroundColor Yellow
dotnet pack QuickScreenshot.csproj -c Release --output ./nupkg

Write-Host "`nBuild completed successfully!" -ForegroundColor Green
Write-Host "- Self-contained executable: ./publish/QuickScreenshot.exe" -ForegroundColor Cyan
Write-Host "- AOT executable (if successful): ./publish-aot/QuickScreenshot.exe" -ForegroundColor Cyan
Write-Host "- NuGet package: ./nupkg/QuickScreenshot.1.0.0.nupkg" -ForegroundColor Cyan
Write-Host "- Test screenshots will be saved to: ./screenshots/" -ForegroundColor Cyan
Write-Host "`nTo install as global tool:" -ForegroundColor Yellow
Write-Host "  dotnet tool install --global --add-source ./nupkg QuickScreenshot" -ForegroundColor White
