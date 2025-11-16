using System.Diagnostics;
using System.Text;
using System.Text.Json;
using FluentSoftwareManager.Models;

namespace FluentSoftwareManager.Services;

public class WingetService
{
    private const string WingetPath = "winget";

    public async Task<List<Package>> SearchPackagesAsync(string query = "")
    {
        var packages = new List<Package>();

        try
        {
            var arguments = string.IsNullOrWhiteSpace(query)
                ? "search --accept-source-agreements"
                : $"search {query} --accept-source-agreements";

            var output = await ExecuteWingetCommandAsync(arguments);
            packages = ParseSearchOutput(output);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error searching packages: {ex.Message}");
        }

        return packages;
    }

    public async Task<List<Package>> GetInstalledPackagesAsync()
    {
        var packages = new List<Package>();

        try
        {
            var output = await ExecuteWingetCommandAsync("list --accept-source-agreements");
            packages = ParseListOutput(output);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting installed packages: {ex.Message}");
        }

        return packages;
    }

    public async Task<List<Package>> GetUpgradeablePackagesAsync()
    {
        var packages = new List<Package>();

        try
        {
            var output = await ExecuteWingetCommandAsync("upgrade --accept-source-agreements");
            packages = ParseUpgradeOutput(output);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting upgradeable packages: {ex.Message}");
        }

        return packages;
    }

    public async Task<Package?> GetPackageInfoAsync(string packageId)
    {
        try
        {
            var output = await ExecuteWingetCommandAsync($"show {packageId} --accept-source-agreements");
            return ParseShowOutput(output, packageId);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error getting package info: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> InstallPackageAsync(string packageId, IProgress<string>? progress = null)
    {
        try
        {
            progress?.Report($"Installing {packageId}...");
            var output = await ExecuteWingetCommandAsync($"install {packageId} --accept-package-agreements --accept-source-agreements", progress);
            progress?.Report("Installation complete!");
            return true;
        }
        catch (Exception ex)
        {
            progress?.Report($"Installation failed: {ex.Message}");
            Debug.WriteLine($"Error installing package: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UninstallPackageAsync(string packageId, IProgress<string>? progress = null)
    {
        try
        {
            progress?.Report($"Uninstalling {packageId}...");
            var output = await ExecuteWingetCommandAsync($"uninstall {packageId} --accept-source-agreements", progress);
            progress?.Report("Uninstallation complete!");
            return true;
        }
        catch (Exception ex)
        {
            progress?.Report($"Uninstallation failed: {ex.Message}");
            Debug.WriteLine($"Error uninstalling package: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> UpgradePackageAsync(string packageId, IProgress<string>? progress = null)
    {
        try
        {
            progress?.Report($"Upgrading {packageId}...");
            var output = await ExecuteWingetCommandAsync($"upgrade {packageId} --accept-package-agreements --accept-source-agreements", progress);
            progress?.Report("Upgrade complete!");
            return true;
        }
        catch (Exception ex)
        {
            progress?.Report($"Upgrade failed: {ex.Message}");
            Debug.WriteLine($"Error upgrading package: {ex.Message}");
            return false;
        }
    }

    private async Task<string> ExecuteWingetCommandAsync(string arguments, IProgress<string>? progress = null)
    {
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
            }
        };

        process.ErrorDataReceived += (sender, e) =>
        {
            if (!string.IsNullOrEmpty(e.Data))
            {
                error.AppendLine(e.Data);
            }
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0 && error.Length > 0)
        {
            throw new Exception(error.ToString());
        }

        return output.ToString();
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
