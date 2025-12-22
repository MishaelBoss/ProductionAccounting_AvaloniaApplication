using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class TabWorkTypeUserControl : UserControl
{
    public TabWorkTypeUserControl()
    {
        InitializeComponent();
        DataContext = new TabWorkTypeUserControlViewModel();
    }
}