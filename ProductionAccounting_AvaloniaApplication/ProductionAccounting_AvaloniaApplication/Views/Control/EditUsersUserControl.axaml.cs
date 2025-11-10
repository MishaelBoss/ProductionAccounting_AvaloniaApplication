using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class EditUsersUserControl : UserControl
{
    public EditUsersUserControl()
    {
        InitializeComponent();
    }

    private void Confirm_Click(object? sender, RoutedEventArgs e) 
    {
        if (DataContext is not EditUsersUserControlViewModel viewModel) return;
        if (viewModel.Upload())
        {
            var parent = this.FindAncestorOfType<UserControl>()?.DataContext as AdminPageUserControlViewModel;
            parent?.CloseAddUsersUserControl();
        }
    }

    private void Cancel_Click(object? sender, RoutedEventArgs e)
    {
        var parent = this.FindAncestorOfType<UserControl>()?.DataContext as AdminPageUserControlViewModel;
        parent?.CloseAddUsersUserControl();
    }
}