using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication;

public partial class CartPositionUserControl : UserControl
{
    public CartPositionUserControl()
    {
        InitializeComponent();
        DataContext = new CartPositionUserControlViewModel();
    }

    private void Delete_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CartPositionUserControlViewModel viewModel) return;

        var confirmDeleteUserViewModel = new ConfirmDeletePositionWindowViewModel()
        {
            Id = viewModel.Id,
            Type = viewModel.Type,
        };

        var confirmDeleteUserWindows = new ConfirmDeletePositionWindow()
        {
            DataContext = confirmDeleteUserViewModel,
        };

        confirmDeleteUserWindows.Show();
    }
}