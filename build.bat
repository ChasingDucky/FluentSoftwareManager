@echo off
REM Fluent Software Manager - Build Script (Batch)
REM For users who prefer CMD over PowerShell

echo =====================================
echo Fluent Software Manager - Build
echo =====================================
echo.

REM Check if dotnet is installed
where dotnet >nul 2>nul
if %ERRORLEVEL% neq 0 (
    echo Error: .NET SDK not found. Please install .NET 8.0 SDK or later.
    echo Download from: https://dotnet.microsoft.com/download
    pause
    exit /b 1
)

echo Checking .NET SDK...
dotnet --version
echo.

REM Set default configuration
set CONFIG=Debug
set PLATFORM=x64

REM Parse arguments
if not "%1"=="" set CONFIG=%1
if not "%2"=="" set PLATFORM=%2

echo Configuration: %CONFIG%
echo Platform: %PLATFORM%
echo.

REM Restore NuGet packages
echo Restoring NuGet packages...
dotnet restore FluentSoftwareManager\FluentSoftwareManager.csproj
if %ERRORLEVEL% neq 0 (
    echo Error: Failed to restore NuGet packages.
    pause
    exit /b 1
)
echo Packages restored successfully!
echo.

REM Build the project
echo Building project...
dotnet build FluentSoftwareManager\FluentSoftwareManager.csproj -c %CONFIG% -p:Platform=%PLATFORM% --no-restore
if %ERRORLEVEL% neq 0 (
    echo Error: Build failed.
    pause
    exit /b 1
)

echo.
echo =====================================
echo Build completed successfully!
echo =====================================
echo.
echo Output: FluentSoftwareManager\bin\%CONFIG%\net8.0-windows10.0.19041.0\%PLATFORM%\
echo.
pause
