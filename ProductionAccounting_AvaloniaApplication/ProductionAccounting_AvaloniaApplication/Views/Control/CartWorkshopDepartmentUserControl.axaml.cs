using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication;

public partial class CartWorkshopDepartmentUserControl : UserControl
{
    public CartWorkshopDepartmentUserControl()
    {
        InitializeComponent();
        DataContext = new CartWorkshopDepartmentUserControlViewModel();
    }
}