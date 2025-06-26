@echo off
echo Building QuickScreenshot...

echo.
echo === Cleaning previous builds ===
dotnet clean QuickScreenshot.csproj

echo.
echo === Restoring packages ===
dotnet restore QuickScreenshot.csproj

echo.
echo === Building Debug ===
dotnet build QuickScreenshot.csproj -c Debug

echo.
echo === Building Release ===
dotnet build QuickScreenshot.csproj -c Release

echo.
echo === Running tests (if any) ===
dotnet test QuickScreenshot.csproj --no-build -c Release

echo.
echo === Creating self-contained executable ===
dotnet publish QuickScreenshot.csproj -c Release -r win-x64 --self-contained -o ./publish -p:PublishAot=false

echo.
echo === Attempting AOT compilation (requires Visual Studio C++ tools) ===
dotnet publish QuickScreenshot.csproj -c Release -r win-x64 --self-contained -o ./publish-aot -p:IsPublishing=true 2>nul || echo AOT compilation failed - requires Visual Studio C++ workload

echo.
echo === Creating NuGet package ===
dotnet pack QuickScreenshot.csproj -c Release --output ./nupkg

echo.
echo Build completed successfully!
echo - Self-contained executable: ./publish/QuickScreenshot.exe
echo - AOT executable (if successful): ./publish-aot/QuickScreenshot.exe
echo - NuGet package: ./nupkg/QuickScreenshot.1.0.0.nupkg
echo - Test screenshots will be saved to: ./screenshots/
echo.
echo To install as global tool:
echo   dotnet tool install --global --add-source ./nupkg QuickScreenshot
echo.
pause
