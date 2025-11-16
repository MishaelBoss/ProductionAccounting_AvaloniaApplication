using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class TabUsersListUserControl : UserControl
{
    public TabUsersListUserControl()
    {
        InitializeComponent();
        DataContext = new TabUsersListUserControlViewModel();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not TabUsersListUserControlViewModel viewModel) return;
        viewModel.HomeMainContent = HomeMainContent;
        viewModel.GetListUsers();
    }

    private void DownloadCloud_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not TabUsersListUserControlViewModel viewModel) return;
        _ = viewModel.DownloadListAsync();
    }

    private void ReturnList_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not TabUsersListUserControlViewModel viewModel) return;
        viewModel.GetListUsers();
    }
}