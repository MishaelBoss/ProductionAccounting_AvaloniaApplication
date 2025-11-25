using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace ProductionAccounting_AvaloniaApplication;

public partial class ConfirmDeleteUserWindow : Window
{
    public ConfirmDeleteUserWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void TopBorder(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}