using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class ProductViewUserControlView : UserControl
{
    public ProductViewUserControlView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ProductViewUserControlViewModel viewModel) return;
        viewModel.SubProductContent = SubProductContent;
        _ = viewModel.LoadSubProductAsync();
    }

    private void Grid_ActualThemeVariantChanged(object? sender, System.EventArgs e)
    {
    }
}