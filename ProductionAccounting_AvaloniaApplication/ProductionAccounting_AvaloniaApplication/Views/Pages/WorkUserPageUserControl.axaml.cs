using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication.Views.Pages;

public partial class WorkUserPageUserControlView : UserControl
{
    public WorkUserPageUserControlView()
    {
        InitializeComponent();
        DataContext = new WorkUserPageUserControlViewModel();
    }
}