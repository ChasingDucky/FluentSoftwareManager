using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentSoftwareManager.Models;
using FluentSoftwareManager.Services;
using Serilog;

namespace FluentSoftwareManager.ViewModels;

public partial class BrowseViewModel : ObservableObject
{
    private readonly WingetService _wingetService;

    [ObservableProperty]
    private ObservableCollection<Package> _packages = new();

    [ObservableProperty]
    private Package? _selectedPackage;

    [ObservableProperty]
    private string _searchQuery = string.Empty;

    [ObservableProperty]
    private bool _isLoading;

    [ObservableProperty]
    private string _statusMessage = string.Empty;

    public BrowseViewModel()
    {
        try
        {
            Log.Debug("BrowseViewModel constructor");
            _wingetService = new WingetService();
            _ = InitializeAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error in BrowseViewModel constructor");
            StatusMessage = "Failed to initialize. Check logs for details.";
        }
    }

    private async Task InitializeAsync()
    {
        try
        {
            Log.Information("Initializing BrowseViewModel");
            await LoadPackagesAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during BrowseViewModel initialization");
            StatusMessage = $"Initialization failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        try
        {
            Log.Information("Search requested: {Query}", SearchQuery);
            await LoadPackagesAsync(SearchQuery);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during search");
            StatusMessage = $"Search failed: {ex.Message}";
        }
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        try
        {
            Log.Information("Refresh requested");
            await LoadPackagesAsync(SearchQuery);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during refresh");
            StatusMessage = $"Refresh failed: {ex.Message}";
            await ErrorDialog.ShowErrorAsync("Refresh Failed", "Failed to refresh package list.", ex);
        }
    }

    [RelayCommand]
    private async Task InstallPackageAsync(Package package)
    {
        if (package == null)
        {
            Log.Warning("InstallPackageAsync called with null package");
            return;
        }

        try
        {
            Log.Information("Installing package: {PackageId}", package.Id);

            IsLoading = true;
            StatusMessage = $"Installing {package.Name}...";

            var progress = new Progress<string>(message =>
            {
                StatusMessage = message;
                Log.Debug("Install progress: {Message}", message);
            });

            var success = await _wingetService.InstallPackageAsync(package.Id, progress);

            if (success)
            {
                package.IsInstalled = true;
                StatusMessage = $"{package.Name} installed successfully!";
                Log.Information("Package installed successfully: {PackageId}", package.Id);

                await ErrorDialog.ShowInfoAsync(
                    "Installation Complete",
                    $"{package.Name} has been installed successfully!"
                );
            }
            else
            {
                StatusMessage = $"Failed to install {package.Name}";
                Log.Warning("Package installation returned false: {PackageId}", package.Id);

                await ErrorDialog.ShowErrorAsync(
                    "Installation Failed",
                    $"Failed to install {package.Name}. Check logs for details."
                );
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception during package installation: {PackageId}", package.Id);
            StatusMessage = $"Installation error: {ex.Message}";

            await ErrorDialog.ShowErrorAsync(
                "Installation Error",
                $"An error occurred while installing {package.Name}.",
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

    private async Task LoadPackagesAsync(string query = "")
    {
        IsLoading = true;
        StatusMessage = string.IsNullOrWhiteSpace(query) ? "Loading packages..." : $"Searching for '{query}'...";

        try
        {
            Log.Information("Loading packages with query: '{Query}'", query);

            var packages = await _wingetService.SearchPackagesAsync(query);

            Packages.Clear();

            if (packages != null && packages.Count > 0)
            {
                foreach (var package in packages)
                {
                    Packages.Add(package);
                }

                StatusMessage = $"Found {packages.Count} package(s)";
                Log.Information("Successfully loaded {Count} packages", packages.Count);
            }
            else
            {
                StatusMessage = "No packages found";
                Log.Information("No packages found for query: '{Query}'", query);
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error loading packages with query: '{Query}'", query);
            StatusMessage = $"Error: {ex.Message}";

            await ErrorDialog.ShowErrorAsync(
                "Load Failed",
                "Failed to load package list. Please check your internet connection and ensure winget is working.",
                ex
            );
        }
        finally
        {
            IsLoading = false;
        }
    }
}
