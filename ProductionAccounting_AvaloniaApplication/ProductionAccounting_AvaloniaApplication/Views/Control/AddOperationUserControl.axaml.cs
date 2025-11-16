using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;
using System.Threading.Tasks;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class AddOperationUserControl : UserControl
{
    public AddOperationUserControl()
    {
        InitializeComponent();
        DataContext = new AddOperationUserControlViewModel();
    }

    private void Confirm_Click(object? sender, RoutedEventArgs e) 
    {
        if (DataContext is not AddOperationUserControlViewModel viewModel) return;
        if (viewModel.Upload())
        {
            var parent = this.FindAncestorOfType<UserControl>()?.DataContext as AdminPageUserControlViewModel;
            parent?.CloseOperationUsersUserControl();
        }
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        var parent = this.FindAncestorOfType<UserControl>()?.DataContext as AdminPageUserControlViewModel;
        parent?.CloseOperationUsersUserControl();
    }

    public async Task RefreshDataAsync()
    {
        if (DataContext is not AddOperationUserControlViewModel viewModel) return;
        viewModel.ClearForm();
    }
}