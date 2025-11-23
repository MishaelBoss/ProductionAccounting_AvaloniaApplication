using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class TabPositionUserControl : UserControl
{
    public TabPositionUserControl()
    {
        InitializeComponent();
        DataContext = new TabPositionUserControlViewModel();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not TabPositionUserControlViewModel viewModel) return;
        viewModel.HomeMainContent = HomeMainContent;
        viewModel.GetList();
    }

    private void ReturnList_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not TabPositionUserControlViewModel viewModel) return;
        viewModel.GetList();
    }
}