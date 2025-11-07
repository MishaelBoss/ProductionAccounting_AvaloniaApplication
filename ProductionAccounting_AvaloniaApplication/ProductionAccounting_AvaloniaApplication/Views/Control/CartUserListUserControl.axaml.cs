using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication;

public partial class CartUserListUserControl : UserControl
{
    public CartUserListUserControl()
    {
        InitializeComponent();
        DataContext = new CartUserListUserControlViewModel();
    }

    private void OpenProfileUser_Click(object? sender, RoutedEventArgs e) 
    { 
    }

    private void OpenWindowsProfileUser_Click(object? sender, RoutedEventArgs e)
    {
    }

    private void OpenWindowsConfirmDeleteUser_Click(object? sender, RoutedEventArgs e)
    {
    }
}