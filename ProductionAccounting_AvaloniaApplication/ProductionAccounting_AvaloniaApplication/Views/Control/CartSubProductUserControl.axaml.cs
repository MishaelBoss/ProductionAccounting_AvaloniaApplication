using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication;

public partial class CartSubProductUserControl : UserControl
{
    public CartSubProductUserControl()
    {
        InitializeComponent();
        DataContext = new CartSubProductUserControlViewModel();
    }
}