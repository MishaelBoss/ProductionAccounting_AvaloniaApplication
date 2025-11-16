using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class CartProductUserControl : UserControl
{
    public CartProductUserControl()
    {
        InitializeComponent();
        DataContext = new CartProductUserControlViewModel();
    }

    private void View_Click(object? sender, RoutedEventArgs e)
    {
    }

    private void Delete_Click(object? sender, RoutedEventArgs e) 
    {
        if (DataContext is not CartProductUserControlViewModel viewModel) return;

        var confirmDeleteViewModel = new ConfirmDeleteProductWindowViewModel()
        {
            Id = viewModel.ProductID,
            Name = viewModel.Name,
        };

        var confirmDeleteWindows = new ConfirmDeleteProductWindow()
        {
            DataContext = confirmDeleteViewModel,
        };

        confirmDeleteWindows.Show();
    }
}