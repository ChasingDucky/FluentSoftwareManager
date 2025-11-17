# Fluent Software Manager - Diagnostic Tool
# Run this script to diagnose startup issues

Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Fluent Software Manager - Diagnostics" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

$issues = @()
$warnings = @()

# Check 1: .NET SDK
Write-Host "[1/8] Checking .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ .NET SDK found: $dotnetVersion" -ForegroundColor Green
    } else {
        $issues += ".NET SDK not found"
        Write-Host "  ✗ .NET SDK not found" -ForegroundColor Red
    }
} catch {
    $issues += ".NET SDK not found"
    Write-Host "  ✗ .NET SDK not found" -ForegroundColor Red
}

# Check 2: .NET Runtimes
Write-Host "[2/8] Checking .NET Runtimes..." -ForegroundColor Yellow
try {
    $runtimes = dotnet --list-runtimes | Select-String "Microsoft.WindowsDesktop.App"
    if ($runtimes) {
        Write-Host "  ✓ WindowsDesktop runtimes found:" -ForegroundColor Green
        $runtimes | ForEach-Object { Write-Host "    $_" -ForegroundColor Gray }
    } else {
        $issues += ".NET Desktop Runtime not found"
        Write-Host "  ✗ .NET Desktop Runtime not found" -ForegroundColor Red
    }
} catch {
    $issues += "Failed to check runtimes"
    Write-Host "  ✗ Failed to check runtimes" -ForegroundColor Red
}

# Check 3: Winget
Write-Host "[3/8] Checking winget..." -ForegroundColor Yellow
try {
    $wingetVersion = winget --version 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "  ✓ Winget found: $wingetVersion" -ForegroundColor Green
    } else {
        $issues += "Winget not found"
        Write-Host "  ✗ Winget not found" -ForegroundColor Red
    }
} catch {
    $issues += "Winget not accessible"
    Write-Host "  ✗ Winget not accessible" -ForegroundColor Red
}

# Check 4: Windows App SDK
Write-Host "[4/8] Checking Windows App SDK..." -ForegroundColor Yellow
try {
    $appSdk = winget list --id Microsoft.WindowsAppRuntime.1.5 2>$null
    if ($appSdk -match "Microsoft.WindowsAppRuntime.1.5") {
        Write-Host "  ✓ Windows App SDK 1.5 found" -ForegroundColor Green
    } else {
        $warnings += "Windows App SDK 1.5 not found (may cause issues)"
        Write-Host "  ⚠ Windows App SDK 1.5 not found" -ForegroundColor Yellow
    }
} catch {
    $warnings += "Could not check Windows App SDK"
    Write-Host "  ⚠ Could not verify Windows App SDK" -ForegroundColor Yellow
}

# Check 5: Administrator privileges
Write-Host "[5/8] Checking administrator privileges..." -ForegroundColor Yellow
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if ($isAdmin) {
    Write-Host "  ✓ Running as administrator" -ForegroundColor Green
} else {
    $warnings += "Not running as administrator (required for install/uninstall)"
    Write-Host "  ⚠ Not running as administrator" -ForegroundColor Yellow
}

# Check 6: Log folder
Write-Host "[6/8] Checking log folder..." -ForegroundColor Yellow
$logPath = "$env:LOCALAPPDATA\FluentSoftwareManager\Logs"
if (Test-Path $logPath) {
    $logFiles = Get-ChildItem $logPath -Filter "*.log" -ErrorAction SilentlyContinue
    Write-Host "  ✓ Log folder exists: $logPath" -ForegroundColor Green
    if ($logFiles) {
        Write-Host "    Found $($logFiles.Count) log file(s)" -ForegroundColor Gray
        $latestLog = $logFiles | Sort-Object LastWriteTime -Descending | Select-Object -First 1
        Write-Host "    Latest: $($latestLog.Name) - $($latestLog.LastWriteTime)" -ForegroundColor Gray
    }
} else {
    Write-Host "  ℹ Log folder not yet created (app hasn't run yet)" -ForegroundColor Cyan
}

# Check 7: Event log errors
Write-Host "[7/8] Checking recent error logs..." -ForegroundColor Yellow
try {
    $recentErrors = Get-EventLog -LogName Application -Newest 5 -EntryType Error -ErrorAction SilentlyContinue |
        Where-Object {$_.TimeGenerated -gt (Get-Date).AddMinutes(-10) -and
                      ($_.Source -like "*FluentSoftwareManager*" -or $_.Source -like "*.NET Runtime*")}

    if ($recentErrors) {
        Write-Host "  ⚠ Found recent errors:" -ForegroundColor Yellow
        $recentErrors | ForEach-Object {
            Write-Host "    $($_.TimeGenerated) - $($_.Source)" -ForegroundColor Gray
        }
        $warnings += "Recent errors found in event log"
    } else {
        Write-Host "  ✓ No recent errors in event log" -ForegroundColor Green
    }
} catch {
    Write-Host "  ℹ Could not check event log" -ForegroundColor Cyan
}

# Check 8: Project files
Write-Host "[8/8] Checking project files..." -ForegroundColor Yellow
$exePath = "FluentSoftwareManager\bin\x64\Debug\net8.0-windows10.0.19041.0\FluentSoftwareManager.exe"
if (Test-Path $exePath) {
    $exe = Get-Item $exePath
    Write-Host "  ✓ Executable found: $($exe.Length) bytes" -ForegroundColor Green
    Write-Host "    Last modified: $($exe.LastWriteTime)" -ForegroundColor Gray
} else {
    $warnings += "Executable not found (project needs to be built)"
    Write-Host "  ⚠ Executable not found - run 'dotnet build' first" -ForegroundColor Yellow
}

# Summary
Write-Host ""
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host "Summary" -ForegroundColor Cyan
Write-Host "=========================================" -ForegroundColor Cyan
Write-Host ""

if ($issues.Count -eq 0 -and $warnings.Count -eq 0) {
    Write-Host "✓ All checks passed!" -ForegroundColor Green
    Write-Host ""
    Write-Host "The application should run successfully." -ForegroundColor Green
    Write-Host "Run with: dotnet run --project FluentSoftwareManager\FluentSoftwareManager.csproj -c Debug -p:Platform=x64" -ForegroundColor Cyan
} else {
    if ($issues.Count -gt 0) {
        Write-Host "Critical Issues Found:" -ForegroundColor Red
        $issues | ForEach-Object { Write-Host "  ✗ $_" -ForegroundColor Red }
        Write-Host ""
    }

    if ($warnings.Count -gt 0) {
        Write-Host "Warnings:" -ForegroundColor Yellow
        $warnings | ForEach-Object { Write-Host "  ⚠ $_" -ForegroundColor Yellow }
        Write-Host ""
    }

    Write-Host "Recommended Actions:" -ForegroundColor Cyan
    Write-Host ""

    if ($issues -match "\.NET") {
        Write-Host "1. Install .NET 8.0 SDK:" -ForegroundColor White
        Write-Host "   winget install Microsoft.DotNet.SDK.8" -ForegroundColor Gray
        Write-Host ""
    }

    if ($issues -match "Desktop Runtime") {
        Write-Host "2. Install .NET Desktop Runtime:" -ForegroundColor White
        Write-Host "   winget install Microsoft.DotNet.DesktopRuntime.8" -ForegroundColor Gray
        Write-Host ""
    }

    if ($issues -match "Winget") {
        Write-Host "3. Install Winget:" -ForegroundColor White
        Write-Host "   - Open Microsoft Store" -ForegroundColor Gray
        Write-Host "   - Install 'App Installer'" -ForegroundColor Gray
        Write-Host "   Or visit: https://github.com/microsoft/winget-cli/releases" -ForegroundColor Gray
        Write-Host ""
    }

    if ($warnings -match "Windows App SDK") {
        Write-Host "4. Install Windows App SDK Runtime:" -ForegroundColor White
        Write-Host "   winget install Microsoft.WindowsAppRuntime.1.5" -ForegroundColor Gray
        Write-Host ""
    }

    if ($warnings -match "administrator") {
        Write-Host "5. Run as administrator:" -ForegroundColor White
        Write-Host "   - Right-click PowerShell -> Run as Administrator" -ForegroundColor Gray
        Write-Host ""
    }

    if ($warnings -match "not found (project") {
        Write-Host "6. Build the project:" -ForegroundColor White
        Write-Host "   dotnet build FluentSoftwareManager\FluentSoftwareManager.csproj -c Debug -p:Platform=x64" -ForegroundColor Gray
        Write-Host ""
    }
}

# Offer to view logs
if (Test-Path $logPath) {
    Write-Host "=========================================" -ForegroundColor Cyan
    Write-Host ""
    $response = Read-Host "Do you want to view the latest log file? (Y/N)"
    if ($response -eq 'Y' -or $response -eq 'y') {
        $latestLog = Get-ChildItem $logPath -Filter "*.log" -ErrorAction SilentlyContinue |
                     Sort-Object LastWriteTime -Descending |
                     Select-Object -First 1
        if ($latestLog) {
            notepad $latestLog.FullName
        }
    }
}

Write-Host ""
Write-Host "For more help, see:" -ForegroundColor Cyan
Write-Host "  - LOGGING.md for logging details" -ForegroundColor Gray
Write-Host "  - BUILD.md for build instructions" -ForegroundColor Gray
Write-Host "  - QUICK_START.md for quick start guide" -ForegroundColor Gray
Write-Host ""
