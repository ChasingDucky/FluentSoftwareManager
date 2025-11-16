using System.Diagnostics;
using System.Text;
using System.Text.Json;
using FluentSoftwareManager.Models;
using Serilog;

namespace FluentSoftwareManager.Services;

public class WingetService
{
    private const string WingetPath = "winget";

    public async Task<List<Package>> SearchPackagesAsync(string query = "")
    {
        Log.Debug("SearchPackagesAsync called with query: {Query}", query);
        var packages = new List<Package>();

        try
        {
            var arguments = string.IsNullOrWhiteSpace(query)
                ? "search --accept-source-agreements"
                : $"search {query} --accept-source-agreements";

            Log.Information("Executing winget search command: {Arguments}", arguments);
            var output = await ExecuteWingetCommandAsync(arguments);
            packages = ParseSearchOutput(output);
            Log.Information("Found {Count} packages", packages.Count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error searching packages with query: {Query}", query);
            throw;
        }

        return packages;
    }

    public async Task<List<Package>> GetInstalledPackagesAsync()
    {
        Log.Debug("GetInstalledPackagesAsync called");
        var packages = new List<Package>();

        try
        {
            Log.Information("Executing winget list command");
            var output = await ExecuteWingetCommandAsync("list --accept-source-agreements");
            packages = ParseListOutput(output);
            Log.Information("Found {Count} installed packages", packages.Count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting installed packages");
            throw;
        }

        return packages;
    }

    public async Task<List<Package>> GetUpgradeablePackagesAsync()
    {
        Log.Debug("GetUpgradeablePackagesAsync called");
        var packages = new List<Package>();

        try
        {
            Log.Information("Executing winget upgrade command");
            var output = await ExecuteWingetCommandAsync("upgrade --accept-source-agreements");
            packages = ParseUpgradeOutput(output);
            Log.Information("Found {Count} upgradeable packages", packages.Count);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting upgradeable packages");
            throw;
        }

        return packages;
    }

    public async Task<Package?> GetPackageInfoAsync(string packageId)
    {
        Log.Debug("GetPackageInfoAsync called for package: {PackageId}", packageId);
        try
        {
            Log.Information("Getting package info for: {PackageId}", packageId);
            var output = await ExecuteWingetCommandAsync($"show {packageId} --accept-source-agreements");
            var package = ParseShowOutput(output, packageId);
            Log.Information("Successfully retrieved package info for: {PackageId}", packageId);
            return package;
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error getting package info for: {PackageId}", packageId);
            return null;
        }
    }

    public async Task<bool> InstallPackageAsync(string packageId, IProgress<string>? progress = null)
    {
        Log.Information("Installing package: {PackageId}", packageId);
        try
        {
            progress?.Report($"Installing {packageId}...");
            var output = await ExecuteWingetCommandAsync($"install {packageId} --accept-package-agreements --accept-source-agreements", progress);
            progress?.Report("Installation complete!");
            Log.Information("Successfully installed package: {PackageId}", packageId);
            return true;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Installation failed: {ex.Message}";
            progress?.Report(errorMsg);
            Log.Error(ex, "Error installing package: {PackageId}", packageId);
            return false;
        }
    }

    public async Task<bool> UninstallPackageAsync(string packageId, IProgress<string>? progress = null)
    {
        Log.Information("Uninstalling package: {PackageId}", packageId);
        try
        {
            progress?.Report($"Uninstalling {packageId}...");
            var output = await ExecuteWingetCommandAsync($"uninstall {packageId} --accept-source-agreements", progress);
            progress?.Report("Uninstallation complete!");
            Log.Information("Successfully uninstalled package: {PackageId}", packageId);
            return true;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Uninstallation failed: {ex.Message}";
            progress?.Report(errorMsg);
            Log.Error(ex, "Error uninstalling package: {PackageId}", packageId);
            return false;
        }
    }

    public async Task<bool> UpgradePackageAsync(string packageId, IProgress<string>? progress = null)
    {
        Log.Information("Upgrading package: {PackageId}", packageId);
        try
        {
            progress?.Report($"Upgrading {packageId}...");
            var output = await ExecuteWingetCommandAsync($"upgrade {packageId} --accept-package-agreements --accept-source-agreements", progress);
            progress?.Report("Upgrade complete!");
            Log.Information("Successfully upgraded package: {PackageId}", packageId);
            return true;
        }
        catch (Exception ex)
        {
            var errorMsg = $"Upgrade failed: {ex.Message}";
            progress?.Report(errorMsg);
            Log.Error(ex, "Error upgrading package: {PackageId}", packageId);
            return false;
        }
    }

    private async Task<string> ExecuteWingetCommandAsync(string arguments, IProgress<string>? progress = null)
    {
        Log.Debug("Executing winget command: {Command} {Arguments}", WingetPath, arguments);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = WingetPath,
                Arguments = arguments,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
                StandardOutputEncoding = Encoding.UTF8
            }
        };

        var output = new StringBuilder();
        var error = new StringBuilder();

        process.OutputDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                output.AppendLine(e.Data);
                progress?.Report(e.Data);
                Log.Verbose("Winget output: {Line}", e.Data);
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                error.AppendLine(e.Data);
                Log.Warning("Winget error output: {Line}", e.Data);
            }
        };

        try
        {
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();

            Log.Debug("Winget command completed with exit code: {ExitCode}", process.ExitCode);

            if (process.ExitCode != 0)
            {
                var errorMessage = error.Length > 0 ? error.ToString() : "Unknown error";
                Log.Error("Winget command failed with exit code {ExitCode}: {Error}", process.ExitCode, errorMessage);
                throw new Exception($"Winget command failed (exit code {process.ExitCode}): {errorMessage}");
            }

            return output.ToString();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception executing winget command: {Command} {Arguments}", WingetPath, arguments);
            throw;
        }
    }

    private List<Package> ParseSearchOutput(string output)
    {
        var packages = new List<Package>();
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        bool dataStarted = false;
        foreach (var line in lines)
        {
            if (line.Contains("---"))
            {
                dataStarted = true;
                continue;
            }

            if (!dataStarted || string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 3)
            {
                var package = new Package
                {
                    Name = parts[0],
                    Id = parts.Length > 1 ? parts[1] : parts[0],
                    Version = parts.Length > 2 ? parts[2] : "Unknown",
                    IsInstalled = false
                };
                packages.Add(package);
            }
        }

        return packages;
    }

    private List<Package> ParseListOutput(string output)
    {
        var packages = new List<Package>();
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        bool dataStarted = false;
        foreach (var line in lines)
        {
            if (line.Contains("---"))
            {
                dataStarted = true;
                continue;
            }

            if (!dataStarted || string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 2)
            {
                var package = new Package
                {
                    Name = parts[0],
                    Id = parts.Length > 1 ? parts[1] : parts[0],
                    InstalledVersion = parts.Length > 2 ? parts[2] : "Unknown",
                    Version = parts.Length > 2 ? parts[2] : "Unknown",
                    IsInstalled = true
                };
                packages.Add(package);
            }
        }

        return packages;
    }

    private List<Package> ParseUpgradeOutput(string output)
    {
        var packages = new List<Package>();
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        bool dataStarted = false;
        foreach (var line in lines)
        {
            if (line.Contains("---"))
            {
                dataStarted = true;
                continue;
            }

            if (!dataStarted || string.IsNullOrWhiteSpace(line))
                continue;

            var parts = line.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 4)
            {
                var package = new Package
                {
                    Name = parts[0],
                    Id = parts[1],
                    InstalledVersion = parts[2],
                    AvailableVersion = parts[3],
                    Version = parts[3],
                    IsInstalled = true
                };
                packages.Add(package);
            }
        }

        return packages;
    }

    private Package? ParseShowOutput(string output, string packageId)
    {
        var package = new Package { Id = packageId };
        var lines = output.Split('\n', StringSplitOptions.RemoveEmptyEntries);

        foreach (var line in lines)
        {
            if (line.StartsWith("Found"))
                continue;

            var colonIndex = line.IndexOf(':');
            if (colonIndex > 0)
            {
                var key = line.Substring(0, colonIndex).Trim();
                var value = line.Substring(colonIndex + 1).Trim();

                switch (key.ToLower())
                {
                    case "name":
                        package.Name = value;
                        break;
                    case "version":
                        package.Version = value;
                        break;
                    case "publisher":
                        package.Publisher = value;
                        break;
                    case "description":
                        package.Description = value;
                        break;
                    case "homepage":
                        package.Homepage = value;
                        break;
                    case "license":
                        package.License = value;
                        break;
                }
            }
        }

        return package;
    }
}
