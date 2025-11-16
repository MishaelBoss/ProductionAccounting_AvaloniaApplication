using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class TabProductUserControl : UserControl
{
    public TabProductUserControl()
    {
        InitializeComponent();
        DataContext = new TabProductUserControlViewModel();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not TabProductUserControlViewModel viewModel) return;
        viewModel.HomeMainContent = HomeMainContent;
        viewModel.GetList();
    }

    private void DownloadCloud_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not TabProductUserControlViewModel viewModel) return;
        _ = viewModel.DownloadListAsync();
    }

    private void ReturnList_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not TabProductUserControlViewModel viewModel) return;
        viewModel.GetList();
    }
}