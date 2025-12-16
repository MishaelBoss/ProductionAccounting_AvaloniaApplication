using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication.Views.Pages;

public partial class AuthorizationPageUserControlView : UserControl
{
    public AuthorizationPageUserControlView()
    {
        InitializeComponent();
        DataContext = new AuthorizationPageUserControlViewModel();
    }
}