using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication.Views.Pages;

public partial class AdminPageUserControlView : UserControl
{
    public AdminPageUserControlView()
    {
        InitializeComponent();
        DataContext = new AdminPageUserControlViewModel();
    }

    private void UserControl_ActualThemeVariantChanged(object? sender, System.EventArgs e)
    {
    }
}