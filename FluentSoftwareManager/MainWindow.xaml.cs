using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using FluentSoftwareManager.Views;

namespace FluentSoftwareManager;

public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        this.InitializeComponent();

        // Set window size
        var hWnd = WinRT.Interop.WindowNative.GetWindowHandle(this);
        var windowId = Microsoft.UI.Win32Interop.GetWindowIdFromWindow(hWnd);
        var appWindow = Microsoft.UI.Windowing.AppWindow.GetFromWindowId(windowId);
        appWindow.Resize(new Windows.Graphics.SizeInt32(1200, 800));

        // Navigate to Browse page by default
        ContentFrame.Navigate(typeof(BrowsePage));
        NavView.SelectedItem = NavView.MenuItems[0];
    }

    private void NavView_SelectionChanged(NavigationView sender, NavigationViewSelectionChangedEventArgs args)
    {
        if (args.IsSettingsSelected)
        {
            ContentFrame.Navigate(typeof(SettingsPage));
        }
        else if (args.SelectedItemContainer != null)
        {
            string tag = args.SelectedItemContainer.Tag?.ToString() ?? "";
            switch (tag)
            {
                case "browse":
                    ContentFrame.Navigate(typeof(BrowsePage));
                    break;
                case "installed":
                    ContentFrame.Navigate(typeof(InstalledPage));
                    break;
                case "updates":
                    ContentFrame.Navigate(typeof(UpdatesPage));
                    break;
            }
        }
    }
}
