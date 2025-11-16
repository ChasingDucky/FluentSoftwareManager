using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace FluentSoftwareManager.Views;

public sealed partial class SettingsPage : Page
{
    public SettingsPage()
    {
        this.InitializeComponent();
    }

    private void ThemeRadioButtons_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (ThemeRadioButtons.SelectedItem is RadioButton selectedRadio)
        {
            string theme = selectedRadio.Tag?.ToString() ?? "Default";

            if (Content is FrameworkElement rootElement)
            {
                switch (theme)
                {
                    case "Light":
                        rootElement.RequestedTheme = ElementTheme.Light;
                        break;
                    case "Dark":
                        rootElement.RequestedTheme = ElementTheme.Dark;
                        break;
                    default:
                        rootElement.RequestedTheme = ElementTheme.Default;
                        break;
                }
            }
        }
    }
}
