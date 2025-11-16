using System.Collections.ObjectModel;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentSoftwareManager.Models;
using FluentSoftwareManager.Services;

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
        _wingetService = new WingetService();
        _ = LoadPackagesAsync();
    }

    [RelayCommand]
    private async Task SearchAsync()
    {
        await LoadPackagesAsync(SearchQuery);
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadPackagesAsync(SearchQuery);
    }

    [RelayCommand]
    private async Task InstallPackageAsync(Package package)
    {
        if (package == null) return;

        IsLoading = true;
        StatusMessage = $"Installing {package.Name}...";

        var progress = new Progress<string>(message => StatusMessage = message);
        var success = await _wingetService.InstallPackageAsync(package.Id, progress);

        if (success)
        {
            package.IsInstalled = true;
            StatusMessage = $"{package.Name} installed successfully!";
        }

        IsLoading = false;
    }

    [RelayCommand]
    private async Task ShowPackageDetailsAsync(Package package)
    {
        if (package == null) return;

        IsLoading = true;
        var detailedPackage = await _wingetService.GetPackageInfoAsync(package.Id);
        if (detailedPackage != null)
        {
            SelectedPackage = detailedPackage;
        }
        IsLoading = false;
    }

    private async Task LoadPackagesAsync(string query = "")
    {
        IsLoading = true;
        StatusMessage = "Loading packages...";

        try
        {
            var packages = await _wingetService.SearchPackagesAsync(query);
            Packages.Clear();
            foreach (var package in packages)
            {
                Packages.Add(package);
            }
            StatusMessage = $"Found {packages.Count} packages";
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsLoading = false;
        }
    }
}
