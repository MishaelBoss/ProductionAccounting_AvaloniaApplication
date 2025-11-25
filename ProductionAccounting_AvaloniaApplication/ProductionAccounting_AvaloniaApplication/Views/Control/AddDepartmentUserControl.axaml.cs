using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication;

public partial class AddDepartmentUserControl : UserControl
{
    public AddDepartmentUserControl()
    {
        InitializeComponent();
        DataContext = new AddDepartmentUserControlViewModel();
    }

    public void RefreshDataAsync()
    {
        if (DataContext is not AddDepartmentUserControlViewModel viewModel) return;
        viewModel.Messageerror = string.Empty;
        viewModel.Department = string.Empty;
    }
}