using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication;

public partial class AdminPageUserControl : UserControl
{
    public AdminPageUserControl()
    {
        InitializeComponent();
        DataContext = new AdminPageUserControlViewModel();
    }
}