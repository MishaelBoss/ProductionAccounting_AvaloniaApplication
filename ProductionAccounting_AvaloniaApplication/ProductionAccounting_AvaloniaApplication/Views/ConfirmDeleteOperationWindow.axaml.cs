using Avalonia.Controls;
using Avalonia.Input;
using ProductionAccounting_AvaloniaApplication.ViewModels;

namespace ProductionAccounting_AvaloniaApplication;

public partial class ConfirmDeleteOperationWindow : Window
{
    public ConfirmDeleteOperationWindow()
    {
        InitializeComponent();
        DataContext = new ConfirmDeleteOperationWindowViewModel();
    }

    private void TopBorder(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}