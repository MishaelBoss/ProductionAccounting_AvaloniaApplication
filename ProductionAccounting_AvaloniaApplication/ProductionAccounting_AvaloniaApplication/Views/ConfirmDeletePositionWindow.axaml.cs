using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels;

namespace ProductionAccounting_AvaloniaApplication;

public partial class ConfirmDeletePositionWindow : Window
{
    public ConfirmDeletePositionWindow()
    {
        InitializeComponent();
        DataContext = this;
    }

    private void Confirm_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not ConfirmDeletePositionWindowViewModel viewModel) return;
        if (viewModel.Delete()) Close();
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