using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class AddOperationUserControl : UserControl
{
    public AddOperationUserControl()
    {
        InitializeComponent();
    }

    public void RefreshData()
    {
        if (DataContext is not AddOperationUserControlViewModel viewModel) return;
        viewModel.ClearForm();
    }
}