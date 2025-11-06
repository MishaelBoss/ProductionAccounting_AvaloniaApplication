using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication;

public partial class WorkUserPageUserControl : UserControl
{
    public WorkUserPageUserControl()
    {
        InitializeComponent();
        DataContext = new WorkUserPageUserControlViewModel();
    }
}