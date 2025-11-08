using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using ProductionAccounting_AvaloniaApplication.ViewModels;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ProductionAccounting_AvaloniaApplication.Views;

namespace ProductionAccounting_AvaloniaApplication;

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
        }
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        var parent = this.FindAncestorOfType<MainWindow>()?.DataContext as MainWindowViewModel;
        parent?.CloseAuthorization();
    }
}