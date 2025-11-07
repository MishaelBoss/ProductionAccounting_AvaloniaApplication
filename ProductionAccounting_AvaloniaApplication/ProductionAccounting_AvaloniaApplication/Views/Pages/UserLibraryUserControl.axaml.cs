using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication.Views.Pages;

public partial class UserLibraryUserControlView : UserControl
{
    public UserLibraryUserControlView()
    {
        InitializeComponent();
        DataContext = new UserLibraryUserControlViewModel();
    }
}