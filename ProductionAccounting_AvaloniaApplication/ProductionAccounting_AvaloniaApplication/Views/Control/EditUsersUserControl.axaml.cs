using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;
using System.Threading.Tasks;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class EditUsersUserControl : UserControl
{
    public EditUsersUserControl()
    {
        InitializeComponent();
    }

    private void Confirm_Click(object? sender, RoutedEventArgs e) 
    {
        if (DataContext is not EditUsersUserControlViewModel viewModel) return;
        if (viewModel.Upload())
        {
            var parent = this.FindAncestorOfType<UserControl>()?.DataContext as AdminPageUserControlViewModel;
            parent?.CloseAddUsersUserControl();
        }
    }

    public async Task RefreshDataAsync()
    {
        if (DataContext is not EditUsersUserControlViewModel viewModel) return;
        viewModel.Messageerror = string.Empty;
        viewModel.ComboBoxItems.Clear();
        viewModel.MiddleName = string.Empty;
        viewModel.FirstUsername = string.Empty;
        viewModel.LastUsername = string.Empty;
        viewModel.BaseSalary = 0;
        await viewModel.LoadListTypeToComboBoxAsync();
        await viewModel.LoadUserDataAsync();
    }
}