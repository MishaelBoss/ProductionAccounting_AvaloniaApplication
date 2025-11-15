using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;
using System.Threading.Tasks;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class AddWorkUserControl : UserControl
{
    public AddWorkUserControl()
    {
        InitializeComponent();
        DataContext = new AddWorkUserControlViewModel();
    }

    private void Confirm_Click(object? sender, RoutedEventArgs e) {
        if (DataContext is not AddWorkUserControlViewModel viewModel) return;
        if (viewModel.Upload()) 
        {
            var parent = this.FindAncestorOfType<UserControl>()?.DataContext as WorkUserPageUserControlViewModel;
            parent?.CloseAddWorkUserControl();
        }
    }

    public async Task RefreshDataAsync()
    {
        if (DataContext is not AddWorkUserControlViewModel viewModel) return;
        viewModel.ClearForm();
        await viewModel.LoadListComboBoxAsync();
    }
}