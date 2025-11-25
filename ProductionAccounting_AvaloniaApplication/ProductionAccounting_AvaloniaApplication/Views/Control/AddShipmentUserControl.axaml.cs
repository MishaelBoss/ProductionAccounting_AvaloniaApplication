using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication;

public partial class AddShipmentUserControl : UserControl
{
    public AddShipmentUserControl()
    {
        InitializeComponent();
        DataContext = new AddShipmentUserControlViewModel();
    }

    public async void RefreshDataAsync()
    {
        if (DataContext is not AddShipmentUserControlViewModel viewModel) return;
        viewModel.Messageerror = string.Empty;
        await viewModel.LoadOrdersAsync();
    }
}