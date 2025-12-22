using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class AddOrderUserControl : UserControl
{
    public AddOrderUserControl()
    {
        InitializeComponent();
        DataContext = new AddOrderUserControlViewModel();
    }
}