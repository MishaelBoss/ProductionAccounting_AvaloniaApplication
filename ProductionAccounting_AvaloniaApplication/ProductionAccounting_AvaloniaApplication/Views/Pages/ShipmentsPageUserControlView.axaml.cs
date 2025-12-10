using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication.Views.Pages;

public partial class ShipmentsPageUserControlView : UserControl
{
    public ShipmentsPageUserControlView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e) 
    {
        if (DataContext is not ShipmentsPageUserControlViewModel viewModel) return;
        viewModel.CartShipment = CartShipment;
        await viewModel.LoadShipmentsAsync();
    }
}