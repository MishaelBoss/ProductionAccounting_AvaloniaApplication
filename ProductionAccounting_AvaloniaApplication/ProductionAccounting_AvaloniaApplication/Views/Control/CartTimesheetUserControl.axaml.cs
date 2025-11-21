using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication;

public partial class CartTimesheetUserControl : UserControl
{
    public CartTimesheetUserControl()
    {
        InitializeComponent();
        DataContext = new CartTimesheetUserControlViewModel();
    }

    private void TextBlock_ActualThemeVariantChanged(object? sender, System.EventArgs e)
    {
    }
}