using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FluentSoftwareManager.Models;
using FluentSoftwareManager.Services;

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
        _wingetService = new WingetService();
        _ = LoadPackagesAsync();
    }

    [RelayCommand]
    private async Task RefreshAsync()
    {
        await LoadPackagesAsync();
    }

    [RelayCommand]
    private async Task UninstallPackageAsync(Package package)
    {
        if (package == null) return;

        IsLoading = true;
        StatusMessage = $"Uninstalling {package.Name}...";

        var progress = new Progress<string>(message => StatusMessage = message);
        var success = await _wingetService.UninstallPackageAsync(package.Id, progress);

        if (success)
        {
            Packages.Remove(package);
            StatusMessage = $"{package.Name} uninstalled successfully!";
        }

        IsLoading = false;
    }

    private async Task LoadPackagesAsync()
    {
        IsLoading = true;
        StatusMessage = "Loading installed packages...";

        try
        {
            var packages = await _wingetService.GetInstalledPackagesAsync();
            Packages.Clear();
            foreach (var package in packages)
            {
                Packages.Add(package);
            }
            StatusMessage = $"Found {packages.Count} installed packages";
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
