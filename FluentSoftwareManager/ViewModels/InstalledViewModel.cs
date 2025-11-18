using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentSoftwareManager.Models;
using FluentSoftwareManager.Services;
using Serilog;

namespace FluentSoftwareManager.ViewModels;

public partial class InstalledViewModel : ObservableObject
{
    private readonly WingetService _wingetService;

    [ObservableProperty]
    private ObservableCollection<Package> _packages = new();

    [ObservableProperty]
    private Package? _selectedPackage;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public InstalledViewModel()
    {
        try
        {
            Log.Debug("InstalledViewModel constructor");
            _wingetService = new WingetService();
            _ = InitializeAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in InstalledViewModel constructor");
            StatusMessage = "Failed to initialize. Check logs for details.";
        }
    }

    private async Task InitializeAsync()
    {
        try
        {
            Log.Information("Initializing InstalledViewModel");
            await LoadPackagesAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during InstalledViewModel initialization");
            StatusMessage = $"Initialization failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            Log.Information("Refresh requested for installed packages");
            await LoadPackagesAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during refresh");
            StatusMessage = $"Refresh failed: {ex.Message}";
            await ErrorDialog.ShowErrorAsync("Refresh Failed", "Failed to refresh installed packages list.", ex);
        }
    }

    [RelayCommand]
    private async Task UninstallPackageAsync(Package package)
    {
        if (package == null)
        {
            Log.Warning("UninstallPackageAsync called with null package");
            return;
        }

        try
        {
            Log.Information("Uninstalling package: {PackageId}", package.Id);

            IsLoading = true;
            StatusMessage = $"Uninstalling {package.Name}...";

            var progress = new Progress<string>(message =>
            {
                StatusMessage = message;
                Log.Debug("Uninstall progress: {Message}", message);
            });

            var success = await _wingetService.UninstallPackageAsync(package.Id, progress);

            if (success)
            {
                Packages.Remove(package);
                StatusMessage = $"{package.Name} uninstalled successfully!";
                Log.Information("Package uninstalled successfully: {PackageId}", package.Id);

                await ErrorDialog.ShowInfoAsync(
                    "Uninstallation Complete",
                    $"{package.Name} has been uninstalled successfully!"
                );
            }
            else
            {
                StatusMessage = $"Failed to uninstall {package.Name}";
                Log.Warning("Package uninstallation returned false: {PackageId}", package.Id);

                await ErrorDialog.ShowErrorAsync(
                    "Uninstallation Failed",
                    $"Failed to uninstall {package.Name}. Check logs for details."
                );
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception during package uninstallation: {PackageId}", package.Id);
            StatusMessage = $"Uninstallation error: {ex.Message}";

            await ErrorDialog.ShowErrorAsync(
                "Uninstallation Error",
                $"An error occurred while uninstalling {package.Name}.",
                ex
            );
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task ShowPackageDetailsAsync(Package package)
    {
        if (package == null)
        {
            Log.Warning("ShowPackageDetailsAsync called with null package");
            return;
        }

        try
        {
            Log.Information("Fetching package details: {PackageId}", package.Id);

            IsLoading = true;
            StatusMessage = $"Loading details for {package.Name}...";

            var detailedPackage = await _wingetService.GetPackageInfoAsync(package.Id);

            if (detailedPackage != null)
            {
                SelectedPackage = detailedPackage;
                await PackageDetailsDialog.ShowAsync(detailedPackage);
                StatusMessage = $"Details loaded for {package.Name}";
                Log.Information("Package details loaded: {PackageId}", package.Id);
            }
            else
            {
                StatusMessage = $"No details available for {package.Name}";
                Log.Warning("No package details returned: {PackageId}", package.Id);
                await ErrorDialog.ShowInfoAsync(
                    "No Details",
                    $"No detailed information is available for {package.Name}."
                );
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error fetching package details: {PackageId}", package.Id);
            StatusMessage = $"Failed to load details: {ex.Message}";

            await ErrorDialog.ShowErrorAsync(
                "Details Error",
                $"Failed to load details for {package.Name}.",
                ex
            );
        }
        finally
        {
            IsLoading = false;
        }
    }

    private async Task LoadPackagesAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading installed packages...";

        try
        {
            Log.Information("Loading installed packages");

            var packages = await _wingetService.GetInstalledPackagesAsync();

            Packages.Clear();

            if (packages != null && packages.Count > 0)
            {
                foreach (var package in packages)
                {
                    Packages.Add(package);
                }

                StatusMessage = $"Found {packages.Count} installed package(s)";
                Log.Information("Successfully loaded {Count} installed packages", packages.Count);
            }
            else
            {
                StatusMessage = "No installed packages found";
                Log.Information("No installed packages found");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading installed packages");
            StatusMessage = $"Error: {ex.Message}";

            await ErrorDialog.ShowErrorAsync(
                "Load Failed",
                "Failed to load installed packages list. Please check your internet connection and ensure winget is working.",
                ex
            );
        }
        finally
        {
            IsLoading = false;
        }
    }
}
