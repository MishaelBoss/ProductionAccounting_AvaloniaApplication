using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class WorkMasterUserControl : UserControl
{
    public WorkMasterUserControl()
    {
        InitializeComponent();
        DataContext = new WorkMasterUserControlViewModel();
    }
}