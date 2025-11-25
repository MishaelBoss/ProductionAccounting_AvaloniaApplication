using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class AddPositionUserControl : UserControl
{
    public AddPositionUserControl()
    {
        InitializeComponent();
        DataContext = new AddPositionUserControlViewModel();
    }

    public void RefreshDataAsync()
    {
        if (DataContext is not AddPositionUserControlViewModel viewModel) return;
        viewModel.Messageerror = string.Empty;
        viewModel.Position = string.Empty;
    }
}