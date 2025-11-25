using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class CartUserListUserControl : UserControl
{
    public CartUserListUserControl()
    {
        InitializeComponent();
        DataContext = new CartUserListUserControlViewModel();
    }
}