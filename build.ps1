# Fluent Software Manager - Build Script
# PowerShell script for building the project

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [Parameter(Mandatory=$false)]
    [ValidateSet('x64', 'x86', 'ARM64')]
    [string]$Platform = 'x64'
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Fluent Software Manager - Build" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check if .NET SDK is installed
Write-Host "Checking .NET SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: .NET SDK not found. Please install .NET 8.0 SDK or later." -ForegroundColor Red
    exit 1
}
Write-Host "Found .NET SDK version: $dotnetVersion" -ForegroundColor Green
Write-Host ""

# Restore NuGet packages
Write-Host "Restoring NuGet packages..." -ForegroundColor Yellow
dotnet restore FluentSoftwareManager/FluentSoftwareManager.csproj
if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Failed to restore NuGet packages." -ForegroundColor Red
    exit 1
}
Write-Host "Packages restored successfully!" -ForegroundColor Green
Write-Host ""

# Build the project
Write-Host "Building project ($Configuration|$Platform)..." -ForegroundColor Yellow
dotnet build FluentSoftwareManager/FluentSoftwareManager.csproj `
    -c $Configuration `
    -p:Platform=$Platform `
    --no-restore

if ($LASTEXITCODE -ne 0) {
    Write-Host "Error: Build failed." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Output location: FluentSoftwareManager/bin/$Configuration/net8.0-windows10.0.19041.0/$Platform/" -ForegroundColor Cyan
