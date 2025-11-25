using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using System.Threading.Tasks;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class EditUsersUserControl : UserControl
{
    public EditUsersUserControl()
    {
        InitializeComponent();
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