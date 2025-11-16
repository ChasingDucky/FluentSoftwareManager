# Fluent Software Manager - Run Script
# PowerShell script for running the project

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [Parameter(Mandatory=$false)]
    [ValidateSet('x64', 'x86', 'ARM64')]
    [string]$Platform = 'x64'
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Fluent Software Manager - Run" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

# Check for administrator privileges
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "Warning: This application requires administrator privileges." -ForegroundColor Yellow
    Write-Host "Some features may not work correctly without admin rights." -ForegroundColor Yellow
    Write-Host ""
    $response = Read-Host "Do you want to restart as administrator? (Y/N)"
    if ($response -eq 'Y' -or $response -eq 'y') {
        Start-Process powershell -Verb RunAs -ArgumentList "-File `"$PSCommandPath`" -Configuration $Configuration -Platform $Platform"
        exit
    }
}

# Build first
Write-Host "Building project..." -ForegroundColor Yellow
& "$PSScriptRoot/build.ps1" -Configuration $Configuration -Platform $Platform
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed. Cannot run the application." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "Starting application..." -ForegroundColor Yellow
Write-Host ""

# Run the application
dotnet run --project FluentSoftwareManager/FluentSoftwareManager.csproj `
    -c $Configuration `
    -p:Platform=$Platform `
    --no-build
