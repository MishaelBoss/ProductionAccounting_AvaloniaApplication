using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication.Views.Pages;

public partial class UserLibraryUserControlView : UserControl
{
    public UserLibraryUserControlView()
    {
        InitializeComponent();
        DataContext = new UserLibraryUserControlViewModel();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e) 
    {
        if (DataContext is not UserLibraryUserControlViewModel viewModel) return;
        viewModel.HomeMainContent = HomeMainContent;
        viewModel.GetListUsers();
    }

    private void DownloadCloud_Click(object? sender, RoutedEventArgs e) 
    {
    }

    private void ReturnList_Click(object? sender, RoutedEventArgs e)
    {
    }
}