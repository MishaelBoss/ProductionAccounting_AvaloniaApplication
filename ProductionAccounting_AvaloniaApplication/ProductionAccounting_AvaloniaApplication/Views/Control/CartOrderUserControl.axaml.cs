using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class CartOrderUserControl : UserControl
{
    public CartOrderUserControl()
    {
        InitializeComponent();
        DataContext = new CartOrderUserControlViewModel();
    }
}