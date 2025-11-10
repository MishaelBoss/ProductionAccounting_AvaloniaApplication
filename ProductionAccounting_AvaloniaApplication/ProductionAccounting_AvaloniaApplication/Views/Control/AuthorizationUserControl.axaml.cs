using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using ProductionAccounting_AvaloniaApplication.ViewModels;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class AuthorizationUserControl : UserControl
{
    public AuthorizationUserControl()
    {
        InitializeComponent();
        DataContext = new AuthorizationUserControlViewModel();
    }

    private void Confirm_Click(object? sender, RoutedEventArgs e) 
    {
        if(DataContext is not AuthorizationUserControlViewModel viewModel) return;
        if (viewModel.Authorization())
        {
            var parent = this.FindAncestorOfType<MainWindow>()?.DataContext as MainWindowViewModel;
            parent?.CloseAuthorization();
            parent?.NotifyLoginStatusChanged();
        }
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        var parent = this.FindAncestorOfType<MainWindow>()?.DataContext as MainWindowViewModel;
        parent?.CloseAuthorization();
    }

    public void RefreshData()
    {
        if (DataContext is not AuthorizationUserControlViewModel viewModel) return;
        viewModel.Messageerror = string.Empty;
        viewModel.Login = string.Empty;
        viewModel.Password = string.Empty;
    }
}