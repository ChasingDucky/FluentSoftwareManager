using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentSoftwareManager.Models;
using FluentSoftwareManager.Services;
using Serilog;

namespace FluentSoftwareManager.ViewModels;

public partial class UpdatesViewModel : ObservableObject
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

    public UpdatesViewModel()
    {
        try
        {
            Log.Debug("UpdatesViewModel constructor");
            _wingetService = new WingetService();
            _ = InitializeAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in UpdatesViewModel constructor");
            StatusMessage = "Failed to initialize. Check logs for details.";
        }
    }

    private async Task InitializeAsync()
    {
        try
        {
            Log.Information("Initializing UpdatesViewModel");
            await LoadPackagesAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during UpdatesViewModel initialization");
            StatusMessage = $"Initialization failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            Log.Information("Refresh requested for updates");
            await LoadPackagesAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during refresh");
            StatusMessage = $"Refresh failed: {ex.Message}";
            await ErrorDialog.ShowErrorAsync("Refresh Failed", "Failed to refresh updates list.", ex);
        }
    }

    [RelayCommand]
    private async Task UpdatePackageAsync(Package package)
    {
        if (package == null)
        {
            Log.Warning("UpdatePackageAsync called with null package");
            return;
        }

        try
        {
            Log.Information("Updating package: {PackageId}", package.Id);

            IsLoading = true;
            StatusMessage = $"Updating {package.Name}...";

            var progress = new Progress<string>(message =>
            {
                StatusMessage = message;
                Log.Debug("Update progress: {Message}", message);
            });

            var success = await _wingetService.UpgradePackageAsync(package.Id, progress);

            if (success)
            {
                Packages.Remove(package);
                StatusMessage = $"{package.Name} updated successfully!";
                Log.Information("Package updated successfully: {PackageId}", package.Id);

                await ErrorDialog.ShowInfoAsync(
                    "Update Complete",
                    $"{package.Name} has been updated successfully!"
                );
            }
            else
            {
                StatusMessage = $"Failed to update {package.Name}";
                Log.Warning("Package update returned false: {PackageId}", package.Id);

                await ErrorDialog.ShowErrorAsync(
                    "Update Failed",
                    $"Failed to update {package.Name}. Check logs for details."
                );
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception during package update: {PackageId}", package.Id);
            StatusMessage = $"Update error: {ex.Message}";

            await ErrorDialog.ShowErrorAsync(
                "Update Error",
                $"An error occurred while updating {package.Name}.",
                ex
            );
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task UpdateAllAsync()
    {
        if (Packages.Count == 0)
        {
            Log.Information("UpdateAllAsync called but no packages available");
            StatusMessage = "No packages to update";
            return;
        }

        try
        {
            Log.Information("Updating all packages, count: {Count}", Packages.Count);

            IsLoading = true;
            StatusMessage = "Updating all packages...";

            var packagesToUpdate = Packages.ToList();
            int successCount = 0;
            int failureCount = 0;

            foreach (var package in packagesToUpdate)
            {
                try
                {
                    Log.Information("Updating package {Index}/{Total}: {PackageId}",
                        packagesToUpdate.IndexOf(package) + 1,
                        packagesToUpdate.Count,
                        package.Id);

                    var progress = new Progress<string>(message =>
                    {
                        StatusMessage = message;
                        Log.Debug("Batch update progress: {Message}", message);
                    });

                    var success = await _wingetService.UpgradePackageAsync(package.Id, progress);

                    if (success)
                    {
                        Packages.Remove(package);
                        successCount++;
                        Log.Information("Package updated successfully: {PackageId}", package.Id);
                    }
                    else
                    {
                        failureCount++;
                        Log.Warning("Package update failed: {PackageId}", package.Id);
                    }
                }
                catch (Exception ex)
                {
                    failureCount++;
                    Log.Error(ex, "Exception updating package: {PackageId}", package.Id);
                }
            }

            StatusMessage = $"Updates complete! Success: {successCount}, Failed: {failureCount}";
            Log.Information("Batch update completed. Success: {Success}, Failed: {Failed}", successCount, failureCount);

            await ErrorDialog.ShowInfoAsync(
                "Batch Update Complete",
                $"Updated {successCount} package(s) successfully.\n{failureCount} package(s) failed to update."
            );
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during batch update");
            StatusMessage = $"Batch update error: {ex.Message}";

            await ErrorDialog.ShowErrorAsync(
                "Batch Update Error",
                "An error occurred during batch update operation.",
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
        StatusMessage = "Checking for updates...";

        try
        {
            Log.Information("Loading upgradeable packages");

            var packages = await _wingetService.GetUpgradeablePackagesAsync();

            Packages.Clear();

            if (packages != null && packages.Count > 0)
            {
                foreach (var package in packages)
                {
                    Packages.Add(package);
                }

                StatusMessage = $"Found {packages.Count} update(s) available";
                Log.Information("Successfully loaded {Count} upgradeable packages", packages.Count);
            }
            else
            {
                StatusMessage = "All packages are up to date!";
                Log.Information("No upgradeable packages found");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading upgradeable packages");
            StatusMessage = $"Error: {ex.Message}";

            await ErrorDialog.ShowErrorAsync(
                "Load Failed",
                "Failed to check for updates. Please check your internet connection and ensure winget is working.",
                ex
            );
        }
        finally
        {
            IsLoading = false;
        }
    }
}
