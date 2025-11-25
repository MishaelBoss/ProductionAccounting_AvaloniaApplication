using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class CartProductUserControl : UserControl
{
    public CartProductUserControl()
    {
        InitializeComponent();
        DataContext = new CartProductUserControlViewModel();
    }
}