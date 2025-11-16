using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;
using System.Threading.Tasks;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class AddProductUserControl : UserControl
{
    public AddProductUserControl()
    {
        InitializeComponent();
        DataContext = new AddProductUserControlViewModel();
    }

    private void Confirm_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not AddProductUserControlViewModel viewModel) return;
        if (viewModel.Upload())
        {
            var parent = this.FindAncestorOfType<UserControl>()?.DataContext as AdminPageUserControlViewModel;
            parent?.CloseProductUsersUserControl();
        }
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        var parent = this.FindAncestorOfType<UserControl>()?.DataContext as AdminPageUserControlViewModel;
        parent?.CloseProductUsersUserControl();
    }

    public async Task RefreshDataAsync()
    {
        if (DataContext is not AddProductUserControlViewModel viewModel) return;
        viewModel.ClearForm();
    }
}