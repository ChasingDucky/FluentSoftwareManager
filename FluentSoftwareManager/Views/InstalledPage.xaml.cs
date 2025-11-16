using Microsoft.UI.Xaml.Controls;
using FluentSoftwareManager.ViewModels;

namespace FluentSoftwareManager.Views;

public sealed partial class InstalledPage : Page
{
    public InstalledViewModel ViewModel { get; }

    public InstalledPage()
    {
        this.InitializeComponent();
        ViewModel = new InstalledViewModel();
    }
}
