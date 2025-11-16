using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentSoftwareManager.Models;
using FluentSoftwareManager.Services;

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
        _wingetService = new WingetService();
        _ = LoadPackagesAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadPackagesAsync();
    }

    [RelayCommand]
    private async Task UpdatePackageAsync(Package package)
    {
        if (package == null) return;

        IsLoading = true;
        StatusMessage = $"Updating {package.Name}...";

        var progress = new Progress<string>(message => StatusMessage = message);
        var success = await _wingetService.UpgradePackageAsync(package.Id, progress);

        if (success)
        {
            Packages.Remove(package);
            StatusMessage = $"{package.Name} updated successfully!";
        }

        IsLoading = false;
    }

    [RelayCommand]
    private async Task UpdateAllAsync()
    {
        IsLoading = true;
        StatusMessage = "Updating all packages...";

        var packagesToUpdate = Packages.ToList();
        foreach (var package in packagesToUpdate)
        {
            var progress = new Progress<string>(message => StatusMessage = message);
            var success = await _wingetService.UpgradePackageAsync(package.Id, progress);
            if (success)
            {
                Packages.Remove(package);
            }
        }

        StatusMessage = "All updates complete!";
        IsLoading = false;
    }

    private async Task LoadPackagesAsync()
    {
        IsLoading = true;
        StatusMessage = "Checking for updates...";

        try
        {
            var packages = await _wingetService.GetUpgradeablePackagesAsync();
            Packages.Clear();
            foreach (var package in packages)
            {
                Packages.Add(package);
            }
            StatusMessage = $"Found {packages.Count} updates available";
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
