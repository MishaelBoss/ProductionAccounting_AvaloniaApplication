using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class AddProductUserControl : UserControl
{
    public AddProductUserControl()
    {
        InitializeComponent();
        DataContext = new AddProductUserControlViewModel();
    }
}