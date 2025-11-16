# Fluent Software Manager - Publish Script
# PowerShell script for publishing the project

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet('x64', 'x86', 'ARM64')]
    [string]$Platform = 'x64',

    [Parameter(Mandatory=$false)]
    [switch]$SelfContained = $false,

    [Parameter(Mandatory=$false)]
    [string]$OutputPath = "./publish"
)

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Fluent Software Manager - Publish" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$runtime = "win-$Platform"
$sc = if ($SelfContained) { "true" } else { "false" }

Write-Host "Configuration:" -ForegroundColor Yellow
Write-Host "  Platform: $Platform" -ForegroundColor White
Write-Host "  Runtime: $runtime" -ForegroundColor White
Write-Host "  Self-Contained: $sc" -ForegroundColor White
Write-Host "  Output: $OutputPath" -ForegroundColor White
Write-Host ""

# Clean output directory
if (Test-Path $OutputPath) {
    Write-Host "Cleaning output directory..." -ForegroundColor Yellow
    Remove-Item -Path $OutputPath -Recurse -Force
}

# Publish the application
Write-Host "Publishing application..." -ForegroundColor Yellow
Write-Host ""

$publishArgs = @(
    'publish',
    'FluentSoftwareManager/FluentSoftwareManager.csproj',
    '-c', 'Release',
    '-r', $runtime,
    '-p:Platform=' + $Platform,
    '-o', $OutputPath,
    '--self-contained', $sc,
    '-p:PublishSingleFile=true',
    '-p:PublishReadyToRun=true',
    '-p:IncludeNativeLibrariesForSelfExtract=true'
)

& dotnet $publishArgs

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Error: Publish failed." -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "Publish completed successfully!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Published files location: $OutputPath" -ForegroundColor Cyan
Write-Host ""

# Display file size
$exePath = Join-Path $OutputPath "FluentSoftwareManager.exe"
if (Test-Path $exePath) {
    $fileSize = (Get-Item $exePath).Length / 1MB
    Write-Host "Executable size: $([math]::Round($fileSize, 2)) MB" -ForegroundColor Cyan
}

Write-Host ""
Write-Host "To run the application:" -ForegroundColor Yellow
Write-Host "  cd $OutputPath" -ForegroundColor White
Write-Host "  .\FluentSoftwareManager.exe" -ForegroundColor White
