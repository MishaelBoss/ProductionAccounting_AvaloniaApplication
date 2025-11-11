using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication;

public partial class InfoItemUserControl : UserControl
{
    public InfoItemUserControl()
    {
        InitializeComponent();
        DataContext = new InfoItemUserControlViewModel();
    }
}