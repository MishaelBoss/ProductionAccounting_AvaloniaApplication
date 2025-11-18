using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication.Views.Pages;

public partial class TimesheetUserControlPageView : UserControl
{
    public TimesheetUserControlPageView()
    {
        InitializeComponent();
        DataContext = new TimesheetUserControlPageViewModel();
    }
}