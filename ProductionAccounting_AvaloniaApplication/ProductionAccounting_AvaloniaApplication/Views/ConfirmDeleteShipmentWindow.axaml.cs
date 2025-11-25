using Avalonia.Controls;
using Avalonia.Input;
using ProductionAccounting_AvaloniaApplication.ViewModels;

namespace ProductionAccounting_AvaloniaApplication.Views;

public partial class ConfirmDeleteShipmentWindow : Window
{
    public ConfirmDeleteShipmentWindow()
    {
        InitializeComponent();
        DataContext = new ConfirmDeleteShipmentWindowViewModel();
    }

    private void TopBorder(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}