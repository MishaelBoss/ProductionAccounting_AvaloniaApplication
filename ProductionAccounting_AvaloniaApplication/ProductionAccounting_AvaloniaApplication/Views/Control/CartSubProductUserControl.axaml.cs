using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class CartSubProductUserControl : UserControl
{
    public CartSubProductUserControl()
    {
        InitializeComponent();
        DataContext = new CartSubProductUserControlViewModel();
    }
}