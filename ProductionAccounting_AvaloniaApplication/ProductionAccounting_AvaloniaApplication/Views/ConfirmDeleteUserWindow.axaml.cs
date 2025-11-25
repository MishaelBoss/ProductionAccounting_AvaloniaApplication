using Avalonia.Controls;
using Avalonia.Input;
using ProductionAccounting_AvaloniaApplication.ViewModels;

namespace ProductionAccounting_AvaloniaApplication;

public partial class ConfirmDeleteUserWindow : Window
{
    public ConfirmDeleteUserWindow()
    {
        InitializeComponent();
        DataContext = new ConfirmDeleteUserWindowViewModel();
    }

    private void TopBorder(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}