using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication;

public partial class CartWorkshopDepartmentUserControl : UserControl
{
    public CartWorkshopDepartmentUserControl()
    {
        InitializeComponent();
        DataContext = new CartWorkshopDepartmentUserControlViewModel();
    }

    private void Delete_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CartWorkshopDepartmentUserControlViewModel viewModel) return;

        var confirmDeleteUserViewModel = new ConfirmDeleteWorkshopDepartmentWindowViewModel()
        {
            Id = viewModel.Id,
            Type = viewModel.Type,
        };

        var confirmDeleteUserWindows = new ConfirmDeleteWorkshopDepartmentWindow()
        {
            DataContext = confirmDeleteUserViewModel,
        };

        confirmDeleteUserWindows.Show();
    }
}