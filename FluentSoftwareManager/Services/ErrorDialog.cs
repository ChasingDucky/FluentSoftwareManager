using Microsoft.UI.Xaml.Controls;
using System.Text;

namespace FluentSoftwareManager.Services;

public static class ErrorDialog
{
    public static async Task ShowErrorAsync(string title, string message, Exception? exception = null)
    {
        var sb = new StringBuilder();
        sb.AppendLine(message);

        if (exception != null)
        {
            sb.AppendLine();
            sb.AppendLine("Error Details:");
            sb.AppendLine(exception.Message);

            if (exception.InnerException != null)
            {
                sb.AppendLine();
                sb.AppendLine("Inner Exception:");
                sb.AppendLine(exception.InnerException.Message);
            }
        }

        var dialog = new ContentDialog
        {
            Title = title,
            Content = new ScrollViewer
            {
                Content = new TextBlock
                {
                    Text = sb.ToString(),
                    TextWrapping = Microsoft.UI.Xaml.TextWrapping.Wrap,
                    IsTextSelectionEnabled = true
                },
                MaxHeight = 400
            },
            CloseButtonText = "OK",
            DefaultButton = ContentDialogButton.Close
        };

        try
        {
            // Try to get the current window's XamlRoot
            var window = App.MainWindow;
            if (window?.Content is Microsoft.UI.Xaml.FrameworkElement element)
            {
                dialog.XamlRoot = element.XamlRoot;
                await dialog.ShowAsync();
            }
        }
        catch
        {
            // Fallback: log the error
            Serilog.Log.Error("Failed to show error dialog: {Title} - {Message}", title, message);
        }
    }

    public static async Task ShowInfoAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            CloseButtonText = "OK",
            DefaultButton = ContentDialogButton.Close
        };

        try
        {
            var window = App.MainWindow;
            if (window?.Content is Microsoft.UI.Xaml.FrameworkElement element)
            {
                dialog.XamlRoot = element.XamlRoot;
                await dialog.ShowAsync();
            }
        }
        catch
        {
            Serilog.Log.Warning("Failed to show info dialog: {Title}", title);
        }
    }

    public static async Task<bool> ShowConfirmAsync(string title, string message)
    {
        var dialog = new ContentDialog
        {
            Title = title,
            Content = message,
            PrimaryButtonText = "Yes",
            CloseButtonText = "No",
            DefaultButton = ContentDialogButton.Close
        };

        try
        {
            var window = App.MainWindow;
            if (window?.Content is Microsoft.UI.Xaml.FrameworkElement element)
            {
                dialog.XamlRoot = element.XamlRoot;
                var result = await dialog.ShowAsync();
                return result == ContentDialogResult.Primary;
            }
        }
        catch
        {
            Serilog.Log.Warning("Failed to show confirm dialog: {Title}", title);
        }

        return false;
    }
}
