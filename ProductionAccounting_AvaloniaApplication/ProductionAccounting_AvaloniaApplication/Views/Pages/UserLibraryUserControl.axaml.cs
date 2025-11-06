using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication;

public partial class UserLibraryUserControl : UserControl
{
    public UserLibraryUserControl()
    {
        InitializeComponent();
        DataContext = new UserLibraryUserControlViewModel();
    }
}