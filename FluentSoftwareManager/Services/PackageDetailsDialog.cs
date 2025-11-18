using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Documents;
using Microsoft.UI.Xaml.Media;
using FluentSoftwareManager.Models;
using Serilog;
using System.Text;

namespace FluentSoftwareManager.Services;

public static class PackageDetailsDialog
{
    public static async Task ShowAsync(Package package)
    {
        if (package == null)
        {
            Log.Warning("ShowAsync called with null package");
            return;
        }

        try
        {
            Log.Information("Showing package details dialog for: {PackageId}", package.Id);

            var dialog = new ContentDialog
            {
                Title = package.Name,
                CloseButtonText = "Close",
                DefaultButton = ContentDialogButton.Close
            };

            // Create a scrollable content area
            var scrollViewer = new ScrollViewer
            {
                MaxHeight = 500,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };

            // Create a StackPanel to hold all details
            var stackPanel = new StackPanel
            {
                Spacing = 12,
                Margin = new Thickness(0, 8, 0, 8)
            };

            // Add package ID
            if (!string.IsNullOrWhiteSpace(package.Id))
            {
                stackPanel.Children.Add(CreateDetailSection("Package ID", package.Id));
            }

            // Add version information
            if (package.IsInstalled)
            {
                if (!string.IsNullOrWhiteSpace(package.InstalledVersion))
                {
                    stackPanel.Children.Add(CreateDetailSection("Installed Version", package.InstalledVersion));
                }
                if (!string.IsNullOrWhiteSpace(package.AvailableVersion) &&
                    package.AvailableVersion != package.InstalledVersion)
                {
                    stackPanel.Children.Add(CreateDetailSection("Available Version", package.AvailableVersion));
                }
            }
            else if (!string.IsNullOrWhiteSpace(package.Version))
            {
                stackPanel.Children.Add(CreateDetailSection("Version", package.Version));
            }

            // Add publisher
            if (!string.IsNullOrWhiteSpace(package.Publisher))
            {
                stackPanel.Children.Add(CreateDetailSection("Publisher", package.Publisher));
            }

            // Add source
            if (!string.IsNullOrWhiteSpace(package.Source))
            {
                stackPanel.Children.Add(CreateDetailSection("Source", package.Source));
            }

            // Add license
            if (!string.IsNullOrWhiteSpace(package.License))
            {
                stackPanel.Children.Add(CreateDetailSection("License", package.License));
            }

            // Add homepage with hyperlink if available
            if (!string.IsNullOrWhiteSpace(package.Homepage))
            {
                stackPanel.Children.Add(CreateHyperlinkSection("Homepage", package.Homepage));
            }

            // Add description (can be long, so put it last)
            if (!string.IsNullOrWhiteSpace(package.Description))
            {
                stackPanel.Children.Add(CreateDetailSection("Description", package.Description, true));
            }

            // Add installation status
            var statusText = package.IsInstalled ? "✓ Installed" : "Not Installed";
            var statusSection = CreateDetailSection("Status", statusText);
            stackPanel.Children.Add(statusSection);

            scrollViewer.Content = stackPanel;
            dialog.Content = scrollViewer;

            // Show the dialog
            var window = App.MainWindow;
            if (window?.Content is FrameworkElement element)
            {
                dialog.XamlRoot = element.XamlRoot;
                await dialog.ShowAsync();
                Log.Debug("Package details dialog shown successfully");
            }
            else
            {
                Log.Warning("Could not get XamlRoot for package details dialog");
            }
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error showing package details dialog");
            await ErrorDialog.ShowErrorAsync(
                "Display Error",
                "Failed to display package details.",
                ex
            );
        }
    }

    private static StackPanel CreateDetailSection(string label, string value, bool isMultiline = false)
    {
        var section = new StackPanel
        {
            Spacing = 4
        };

        // Label
        var labelTextBlock = new TextBlock
        {
            Text = label,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray)
        };
        section.Children.Add(labelTextBlock);

        // Value
        var valueTextBlock = new TextBlock
        {
            Text = value,
            TextWrapping = isMultiline ? TextWrapping.Wrap : TextWrapping.NoWrap,
            IsTextSelectionEnabled = true,
            FontSize = 14
        };
        section.Children.Add(valueTextBlock);

        return section;
    }

    private static StackPanel CreateHyperlinkSection(string label, string url)
    {
        var section = new StackPanel
        {
            Spacing = 4
        };

        // Label
        var labelTextBlock = new TextBlock
        {
            Text = label,
            FontWeight = Microsoft.UI.Text.FontWeights.SemiBold,
            Foreground = new SolidColorBrush(Microsoft.UI.Colors.Gray)
        };
        section.Children.Add(labelTextBlock);

        // Hyperlink
        var hyperlinkButton = new HyperlinkButton
        {
            Content = url,
            NavigateUri = Uri.TryCreate(url, UriKind.Absolute, out var uri) ? uri : null,
            Padding = new Thickness(0)
        };
        section.Children.Add(hyperlinkButton);

        return section;
    }

    public static async Task<bool> ShowInstallConfirmationAsync(Package package)
    {
        if (package == null)
        {
            Log.Warning("ShowInstallConfirmationAsync called with null package");
            return false;
        }

        try
        {
            var message = new StringBuilder();
            message.AppendLine($"Install {package.Name}?");
            message.AppendLine();
            message.AppendLine($"Publisher: {package.Publisher}");
            if (!string.IsNullOrWhiteSpace(package.Version))
            {
                message.AppendLine($"Version: {package.Version}");
            }

            return await ErrorDialog.ShowConfirmAsync("Confirm Installation", message.ToString());
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error showing install confirmation dialog");
            return false;
        }
    }

    public static async Task<bool> ShowUninstallConfirmationAsync(Package package)
    {
        if (package == null)
        {
            Log.Warning("ShowUninstallConfirmationAsync called with null package");
            return false;
        }

        try
        {
            var message = $"Are you sure you want to uninstall {package.Name}?";
            if (!string.IsNullOrWhiteSpace(package.InstalledVersion))
            {
                message += $"\n\nInstalled Version: {package.InstalledVersion}";
            }

            return await ErrorDialog.ShowConfirmAsync("Confirm Uninstallation", message);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error showing uninstall confirmation dialog");
            return false;
        }
    }

    public static async Task<bool> ShowUpdateConfirmationAsync(Package package)
    {
        if (package == null)
        {
            Log.Warning("ShowUpdateConfirmationAsync called with null package");
            return false;
        }

        try
        {
            var message = new StringBuilder();
            message.AppendLine($"Update {package.Name}?");
            message.AppendLine();
            if (!string.IsNullOrWhiteSpace(package.InstalledVersion))
            {
                message.AppendLine($"Current Version: {package.InstalledVersion}");
            }
            if (!string.IsNullOrWhiteSpace(package.AvailableVersion))
            {
                message.AppendLine($"New Version: {package.AvailableVersion}");
            }

            return await ErrorDialog.ShowConfirmAsync("Confirm Update", message.ToString());
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Error showing update confirmation dialog");
            return false;
        }
    }
}
