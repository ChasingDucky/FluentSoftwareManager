@echo off
REM Fluent Software Manager - Diagnostic Tool (Batch version)
REM For users who prefer CMD

echo =========================================
echo Fluent Software Manager - Diagnostics
echo =========================================
echo.

echo [1/8] Checking .NET SDK...
where dotnet >nul 2>nul
if %ERRORLEVEL% equ 0 (
    dotnet --version
    echo   OK: .NET SDK found
) else (
    echo   ERROR: .NET SDK not found
    echo   Install with: winget install Microsoft.DotNet.SDK.8
)
echo.

echo [2/8] Checking .NET Runtimes...
dotnet --list-runtimes | findstr "WindowsDesktop" >nul 2>nul
if %ERRORLEVEL% equ 0 (
    echo   OK: WindowsDesktop Runtime found
) else (
    echo   ERROR: .NET Desktop Runtime not found
    echo   Install with: winget install Microsoft.DotNet.DesktopRuntime.8
)
echo.

echo [3/8] Checking winget...
winget --version >nul 2>nul
if %ERRORLEVEL% equ 0 (
    echo   OK: Winget found
) else (
    echo   ERROR: Winget not found
    echo   Install from Microsoft Store: App Installer
)
echo.

echo [4/8] Checking Windows App SDK...
winget list --id Microsoft.WindowsAppRuntime.1.5 >nul 2>nul
if %ERRORLEVEL% equ 0 (
    echo   OK: Windows App SDK found
) else (
    echo   WARNING: Windows App SDK not found
    echo   Install with: winget install Microsoft.WindowsAppRuntime.1.5
)
echo.

echo [5/8] Checking administrator privileges...
net session >nul 2>&1
if %ERRORLEVEL% equ 0 (
    echo   OK: Running as administrator
) else (
    echo   WARNING: Not running as administrator
    echo   Right-click and "Run as administrator" for full functionality
)
echo.

echo [6/8] Checking log folder...
if exist "%LOCALAPPDATA%\FluentSoftwareManager\Logs" (
    echo   OK: Log folder exists
    dir /b /o-d "%LOCALAPPDATA%\FluentSoftwareManager\Logs\*.log" 2>nul | findstr /r ".*" >nul
    if %ERRORLEVEL% equ 0 (
        echo   Found log files
    )
) else (
    echo   INFO: Log folder not yet created
)
echo.

echo [7/8] Checking project files...
if exist "FluentSoftwareManager\bin\x64\Debug\net8.0-windows10.0.19041.0\FluentSoftwareManager.exe" (
    echo   OK: Executable found
) else (
    echo   WARNING: Executable not found
    echo   Run: dotnet build FluentSoftwareManager\FluentSoftwareManager.csproj -c Debug -p:Platform=x64
)
echo.

echo [8/8] Checking for recent errors...
echo   (Check Event Viewer manually for detailed error logs)
echo.

echo =========================================
echo Diagnostic complete
echo =========================================
echo.
echo For detailed diagnostics, run: diagnose.ps1 in PowerShell
echo For help, see: QUICK_START.md
echo.
pause
