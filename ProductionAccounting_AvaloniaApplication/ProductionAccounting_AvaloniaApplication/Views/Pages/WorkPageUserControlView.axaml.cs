using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication.Views.Pages;

public partial class WorkPageUserControlView : UserControl
{
    public WorkPageUserControlView()
    {
        InitializeComponent();
        DataContext = new WorkPageUserControlViewModel();
    }
}