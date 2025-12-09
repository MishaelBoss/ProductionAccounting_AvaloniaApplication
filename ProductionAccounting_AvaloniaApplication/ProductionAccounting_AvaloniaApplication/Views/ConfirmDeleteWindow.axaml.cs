using Avalonia.Controls;
using Avalonia.Input;

namespace ProductionAccounting_AvaloniaApplication;

public partial class ConfirmDeleteWindow : Window
{
    public ConfirmDeleteWindow()
    {
        InitializeComponent();
    }

    private void TopBorder(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}