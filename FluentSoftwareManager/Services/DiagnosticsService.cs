using System.Diagnostics;
using System.Text;
using Serilog;

namespace FluentSoftwareManager.Services;

public static class DiagnosticsService
{
    public static async Task<string> GenerateDiagnosticReportAsync()
    {
        var sb = new StringBuilder();

        sb.AppendLine("=== Fluent Software Manager - Diagnostic Report ===");
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();

        // Application Info
        sb.AppendLine("=== Application Info ===");
        sb.AppendLine($"Version: {LogService.GetAppVersion()}");
        sb.AppendLine($"Log Folder: {LogService.GetLogFolder()}");
        sb.AppendLine();

        // System Info
        sb.AppendLine("=== System Info ===");
        sb.AppendLine($"OS: {Environment.OSVersion}");
        sb.AppendLine($"Machine: {Environment.MachineName}");
        sb.AppendLine($"User: {Environment.UserName}");
        sb.AppendLine($"64-bit OS: {Environment.Is64BitOperatingSystem}");
        sb.AppendLine($"64-bit Process: {Environment.Is64BitProcess}");
        sb.AppendLine($".NET Version: {Environment.Version}");
        sb.AppendLine();

        // .NET Runtime Info
        sb.AppendLine("=== .NET Runtime Info ===");
        var dotnetInfo = await RunCommandAsync("dotnet", "--info");
        sb.AppendLine(dotnetInfo.Output);
        sb.AppendLine();

        // Winget Info
        sb.AppendLine("=== Winget Info ===");
        var wingetVersion = await RunCommandAsync("winget", "--version");
        if (wingetVersion.Success)
        {
            sb.AppendLine($"Version: {wingetVersion.Output}");
        }
        else
        {
            sb.AppendLine("⚠ Winget not found or not accessible");
            sb.AppendLine($"Error: {wingetVersion.Error}");
        }
        sb.AppendLine();

        // Windows App SDK
        sb.AppendLine("=== Windows App SDK ===");
        var appSdk = await RunCommandAsync("winget", "list --id Microsoft.WindowsAppRuntime.1.5");
        sb.AppendLine(appSdk.Success ? appSdk.Output : "Not found");
        sb.AppendLine();

        // Memory Info
        sb.AppendLine("=== Memory Info ===");
        using (var process = Process.GetCurrentProcess())
        {
            sb.AppendLine($"Working Set: {process.WorkingSet64 / 1024 / 1024} MB");
            sb.AppendLine($"Private Memory: {process.PrivateMemorySize64 / 1024 / 1024} MB");
            sb.AppendLine($"Virtual Memory: {process.VirtualMemorySize64 / 1024 / 1024} MB");
        }
        sb.AppendLine();

        // Recent Logs
        sb.AppendLine("=== Recent Log Entries ===");
        try
        {
            var logFiles = Directory.GetFiles(LogService.GetLogFolder(), "app*.log")
                .OrderByDescending(f => new FileInfo(f).LastWriteTime)
                .Take(1);

            foreach (var logFile in logFiles)
            {
                var lines = File.ReadAllLines(logFile).TakeLast(20);
                foreach (var line in lines)
                {
                    sb.AppendLine(line);
                }
            }
        }
        catch (Exception ex)
        {
            sb.AppendLine($"Error reading logs: {ex.Message}");
        }

        return sb.ToString();
    }

    public static async Task SaveDiagnosticReportAsync(string filePath)
    {
        try
        {
            var report = await GenerateDiagnosticReportAsync();
            await File.WriteAllTextAsync(filePath, report);
            Log.Information("Diagnostic report saved to {FilePath}", filePath);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to save diagnostic report to {FilePath}", filePath);
            throw;
        }
    }

    private static async Task<CommandResult> RunCommandAsync(string command, string arguments)
    {
        try
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = command,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process { StartInfo = processInfo };
            var outputBuilder = new StringBuilder();
            var errorBuilder = new StringBuilder();

            process.OutputDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    outputBuilder.AppendLine(e.Data);
            };

            process.ErrorDataReceived += (sender, e) =>
            {
                if (!string.IsNullOrEmpty(e.Data))
                    errorBuilder.AppendLine(e.Data);
            };

            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();

            await process.WaitForExitAsync();

            return new CommandResult
            {
                Success = process.ExitCode == 0,
                Output = outputBuilder.ToString(),
                Error = errorBuilder.ToString()
            };
        }
        catch (Exception ex)
        {
            return new CommandResult
            {
                Success = false,
                Output = string.Empty,
                Error = ex.Message
            };
        }
    }

    private class CommandResult
    {
        public bool Success { get; set; }
        public string Output { get; set; } = string.Empty;
        public string Error { get; set; } = string.Empty;
    }
}
