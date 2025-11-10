using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.ViewModels;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using System.Threading.Tasks;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class ProfileUserUserControl : UserControl
{
    public ProfileUserUserControl()
    {
        InitializeComponent();
        DataContext = new ProfileUserUserControlViewModel();
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e) 
    {
        var parent = this.FindAncestorOfType<MainWindow>()?.DataContext as MainWindowViewModel;
        parent?.CloseProfileUser();
    }

    private void Edit_Click(object? sender, RoutedEventArgs e)
    {
    }

    private void Logout_Click(object? sender, RoutedEventArgs e)
    {
        ManagerCookie.DeleteCookie();
        
        var parent = this.FindAncestorOfType<MainWindow>()?.DataContext as MainWindowViewModel;
        parent?.CloseProfileUser();
        parent?.NotifyLoginStatusChanged();
    }

    public async Task RefreshDataAsync()
    {
        if (DataContext is not ProfileUserUserControlViewModel viewModel) return;
        await viewModel.LoadDateAsync();
    }
}