@echo off
echo Installing QuickScreenshot as a global .NET tool...

echo.
echo === Building and packing ===
call build.bat

echo.
echo === Installing globally ===
dotnet tool uninstall --global QuickScreenshot 2>nul || echo No previous installation found
dotnet tool install --global --add-source ./nupkg QuickScreenshot

echo.
echo Installation completed!
echo You can now use 'qscreenshot' from anywhere in your command line.
echo.
echo Examples:
echo   qscreenshot
echo   qscreenshot --window "Visual Studio Code"  
echo   qscreenshot --list-windows
echo   qscreenshot --help
echo.
pause
