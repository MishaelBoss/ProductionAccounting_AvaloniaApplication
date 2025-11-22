using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication;

public partial class CartTaskUserControl : UserControl
{
    public CartTaskUserControl()
    {
        InitializeComponent();
        DataContext = new CartTaskUserControlViewModel();
    }
}