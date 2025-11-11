using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication;

public partial class AddPositionUserControl : UserControl
{
    public AddPositionUserControl()
    {
        InitializeComponent();
        DataContext = new AddPositionUserControlViewModel();
    }

    private void Confirm_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not AddPositionUserControlViewModel viewModel) return;
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

    public void RefreshDataAsync()
    {
        if (DataContext is not AddPositionUserControlViewModel viewModel) return;
        viewModel.Messageerror = string.Empty;
        viewModel.Position = string.Empty;
    }
}