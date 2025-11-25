using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication;

public partial class CartPositionUserControl : UserControl
{
    public CartPositionUserControl()
    {
        InitializeComponent();
        DataContext = new CartPositionUserControlViewModel();
    }
}