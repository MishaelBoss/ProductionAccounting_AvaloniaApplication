using Avalonia.Controls;
using Avalonia.Input;

namespace ProductionAccounting_AvaloniaApplication.Views;

public partial class ConfirmDeleteShipmentWindow : Window
{
    public ConfirmDeleteShipmentWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void TopBorder(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}