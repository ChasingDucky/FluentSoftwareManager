# Fluent Software Manager - Clean Script
# PowerShell script for cleaning build artifacts

Write-Host "=====================================" -ForegroundColor Cyan
Write-Host "Fluent Software Manager - Clean" -ForegroundColor Cyan
Write-Host "=====================================" -ForegroundColor Cyan
Write-Host ""

$projectPath = "FluentSoftwareManager"
$foldersToClean = @(
    "$projectPath/bin",
    "$projectPath/obj",
    "publish"
)

foreach ($folder in $foldersToClean) {
    if (Test-Path $folder) {
        Write-Host "Removing: $folder" -ForegroundColor Yellow
        Remove-Item -Path $folder -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "  ✓ Removed" -ForegroundColor Green
    } else {
        Write-Host "Skipping: $folder (not found)" -ForegroundColor Gray
    }
}

Write-Host ""
Write-Host "=====================================" -ForegroundColor Green
Write-Host "Clean completed!" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
