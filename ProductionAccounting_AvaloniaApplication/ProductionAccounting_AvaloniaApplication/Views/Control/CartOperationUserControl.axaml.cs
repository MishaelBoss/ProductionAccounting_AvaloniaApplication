using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication;

public partial class CartOperationUserControl : UserControl
{
    public CartOperationUserControl()
    {
        InitializeComponent();
        DataContext = new CartOperationUserControlViewModel();
    }

    private void View_Click(object? sender, RoutedEventArgs e)
    {
    }

    private void Delete_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CartOperationUserControlViewModel viewModel) return;

        var confirmDeleteViewModel = new ConfirmDeleteOperationWindowViewModel()
        {
            Id = viewModel.OperationID,
            Name = viewModel.Name,
        };

        var confirmDeleteWindows = new ConfirmDeleteOperationWindow()
        {
            DataContext = confirmDeleteViewModel,
        };

        confirmDeleteWindows.Show();
    }
}