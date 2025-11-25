using Avalonia.Controls;
using Avalonia.Input;
using ProductionAccounting_AvaloniaApplication.ViewModels;

namespace ProductionAccounting_AvaloniaApplication;

public partial class ConfirmDeletePositionWindow : Window
{
    public ConfirmDeletePositionWindow()
    {
        InitializeComponent();
        DataContext = new ConfirmDeletePositionWindowViewModel();
    }

    private void TopBorder(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}