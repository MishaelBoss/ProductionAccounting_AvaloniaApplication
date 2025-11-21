using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using ProductionAccounting_AvaloniaApplication.ViewModels;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class CartUserListUserControl : UserControl
{
    public CartUserListUserControl()
    {
        InitializeComponent();
        DataContext = new CartUserListUserControlViewModel();
    }

    private void OpenProfileUser_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CartUserListUserControlViewModel viewModel) return;

        var parentAdminViewModel = this.FindAncestorOfType<UserControl>()?.DataContext as TabUsersListUserControlViewModel;
        parentAdminViewModel?.IsProfileView = parentAdminViewModel.IsProfileView ? false : true;
        parentAdminViewModel?.InitDateUserAsync(viewModel.UserID);
    }

    private void OpenWindowsConfirmDeleteUser_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not CartUserListUserControlViewModel viewModel) return;

        var confirmDeleteUserViewModel = new ConfirmDeleteUserWindowViewModel()
        {
            UserID = viewModel.UserID,
            Login = viewModel.Login,
        };

        var confirmDeleteUserWindows = new ConfirmDeleteUserWindow()
        {
            DataContext = confirmDeleteUserViewModel,
        };

        confirmDeleteUserWindows.Show();
    }
}