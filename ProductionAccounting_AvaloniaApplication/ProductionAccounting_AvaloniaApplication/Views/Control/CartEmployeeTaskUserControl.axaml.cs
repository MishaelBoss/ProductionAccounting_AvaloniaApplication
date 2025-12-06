using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class CartEmployeeTaskUserControl : UserControl
{
    public CartEmployeeTaskUserControl()
    {
        InitializeComponent();
        DataContext = new CartEmployeeTaskUserControlViewModel();
    }
}