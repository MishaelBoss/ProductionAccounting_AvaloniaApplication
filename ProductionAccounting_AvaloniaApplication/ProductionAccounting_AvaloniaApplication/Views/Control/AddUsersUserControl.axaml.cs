using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using System.Threading.Tasks;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class AddUsersUserControl : UserControl
{
    public AddUsersUserControl()
    {
        InitializeComponent();
        DataContext = new AddUsersUserControlViewModel();
    }

    public async Task RefreshDataAsync()
    {
        if (DataContext is not AddUsersUserControlViewModel viewModel) return;
        viewModel.Messageerror = string.Empty;
        viewModel.ComboBoxItems.Clear();
        viewModel.MiddleName = string.Empty;
        viewModel.FirstUsername = string.Empty;
        viewModel.LastUsername = string.Empty;
        viewModel.BaseSalary = 0;
        await viewModel.LoadListTypeToComboBoxAsync();
    }
}