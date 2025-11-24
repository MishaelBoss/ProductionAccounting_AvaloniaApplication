using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class CartShipmentUserControl : UserControl
{
    public CartShipmentUserControl()
    {
        InitializeComponent();
        DataContext = new CartShipmentUserControlViewModel();
    }
}