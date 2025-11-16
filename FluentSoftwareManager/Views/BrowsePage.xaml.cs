using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using FluentSoftwareManager.ViewModels;
using FluentSoftwareManager.Models;

namespace FluentSoftwareManager.Views;

public sealed partial class BrowsePage : Page
{
    public BrowseViewModel ViewModel { get; }

    public BrowsePage()
    {
        this.InitializeComponent();
        ViewModel = new BrowseViewModel();
    }

    private void SearchBox_QuerySubmitted(AutoSuggestBox sender, AutoSuggestBoxQuerySubmittedEventArgs args)
    {
        ViewModel.SearchCommand.Execute(null);
    }
}
