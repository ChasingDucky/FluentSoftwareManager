using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using FluentSoftwareManager.Views;
using Serilog;
using System;
using System.Diagnostics;

namespace FluentSoftwareManager;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        try
        {
            Log.Debug("MainWindow constructor started");

            this.InitializeComponent();
            Log.Debug("XAML components initialized");

            // Set window size with error handling
            try
            {
                var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
                var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
                var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);

                if (appWindow != null)
                {
                    appWindow.Resize(new Windows.Graphics.SizeInt32(1200, 800));
                    Log.Information("Window resized to 1200x800");
                }
                else
                {
                    Log.Warning("Failed to get AppWindow, using default size");
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to resize window, using default size");
                // Continue anyway - this is not critical
            }

            // Set window title
            try
            {
                this.Title = "Fluent Software Manager";
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Failed to set window title");
            }

            // Navigate to Browse page by default with error handling
            try
            {
                bool navigated = ContentFrame.Navigate(typeof(BrowsePage));
                if (navigated)
                {
                    Log.Information("Navigated to BrowsePage");
                    NavView.SelectedItem = NavView.MenuItems[0];
                }
                else
                {
                    Log.Error("Failed to navigate to BrowsePage");
                    ShowNavigationError();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Exception during initial navigation");
                ShowNavigationError();
            }

            Log.Information("MainWindow initialized successfully");
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Fatal error in MainWindow constructor");
            Debug.WriteLine($"FATAL MainWindow error: {ex.Message}");
            Debug.WriteLine($"Stack: {ex.StackTrace}");
            throw;
        }
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        try
        {
            if (args.IsSettingsSelected)
            {
                Log.Debug("Navigating to Settings");
                NavigateToPage(typeof(SettingsPage), "Settings");
            }
            else if (args.SelectedItemContainer != null)
            {
                string tag = args.SelectedItemContainer.Tag?.ToString() ?? "";
                Log.Debug("Navigation requested: {Tag}", tag);

                switch (tag)
                {
                    case "browse":
                        NavigateToPage(typeof(BrowsePage), "Browse");
                        break;
                    case "installed":
                        NavigateToPage(typeof(InstalledPage), "Installed");
                        break;
                    case "updates":
                        NavigateToPage(typeof(UpdatesPage), "Updates");
                        break;
                    default:
                        Log.Warning("Unknown navigation tag: {Tag}", tag);
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error during navigation");
            ShowNavigationError();
        }
    }

    private void NavigateToPage(Type pageType, string pageName)
    {
        try
        {
            bool navigated = ContentFrame.Navigate(pageType);
            if (navigated)
            {
                Log.Information("Successfully navigated to {Page}", pageName);
            }
            else
            {
                Log.Error("Failed to navigate to {Page}", pageName);
                ShowNavigationError();
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Exception navigating to {Page}", pageName);
            ShowNavigationError();
        }
    }

    private async void ShowNavigationError()
    {
        try
        {
            var dialog = new ContentDialog
            {
                Title = "Navigation Error",
                Content = "Failed to load the requested page. The application may be unstable.",
                CloseButtonText = "OK",
                XamlRoot = this.Content.XamlRoot
            };

            await dialog.ShowAsync();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to show navigation error dialog");
            Debug.WriteLine($"Navigation error dialog failed: {ex.Message}");
        }
    }
}
