using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication.Views.Pages;

public partial class UserLibraryPageUserControlView : UserControl
{
    public UserLibraryPageUserControlView()
    {
        InitializeComponent();
        DataContext = new UserLibraryPageUserControlViewModel();
    }
}