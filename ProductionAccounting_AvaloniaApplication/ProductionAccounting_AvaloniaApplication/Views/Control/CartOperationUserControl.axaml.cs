using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication;

public partial class CartOperationUserControl : UserControl
{
    public CartOperationUserControl()
    {
        InitializeComponent();
        DataContext = new CartOperationUserControlViewModel();
    }
}