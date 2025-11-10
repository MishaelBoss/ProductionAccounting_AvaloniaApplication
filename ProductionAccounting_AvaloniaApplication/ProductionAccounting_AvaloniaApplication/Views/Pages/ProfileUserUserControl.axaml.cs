using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication;

public partial class ProfileUserUserControl : UserControl
{
    public ProfileUserUserControl()
    {
        InitializeComponent();
        DataContext = new ProfileUserUserControlViewModel();
    }
}