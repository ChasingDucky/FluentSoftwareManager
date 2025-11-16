using Microsoft.UI.Xaml.Controls;
using FluentSoftwareManager.ViewModels;

namespace FluentSoftwareManager.Views;

public sealed partial class UpdatesPage : Page
{
    public UpdatesViewModel ViewModel { get; }

    public UpdatesPage()
    {
        this.InitializeComponent();
        ViewModel = new UpdatesViewModel();
    }
}
