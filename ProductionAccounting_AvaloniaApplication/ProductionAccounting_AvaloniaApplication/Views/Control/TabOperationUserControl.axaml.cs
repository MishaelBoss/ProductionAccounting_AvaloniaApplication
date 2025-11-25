using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class TabOperationUserControl : UserControl
{
    public TabOperationUserControl()
    {
        InitializeComponent();
        DataContext = new TabOperationUserControlViewModel();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not TabOperationUserControlViewModel viewModel) return;
        viewModel.HomeMainContent = HomeMainContent;
        viewModel.GetList();
    }
}