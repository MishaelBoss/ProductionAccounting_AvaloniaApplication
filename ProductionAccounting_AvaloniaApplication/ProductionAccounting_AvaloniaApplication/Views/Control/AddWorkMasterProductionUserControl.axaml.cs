using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;
using System.Threading.Tasks;

namespace ProductionAccounting_AvaloniaApplication;

public partial class AddWorkMasterProductionUserControl : UserControl
{
    public AddWorkMasterProductionUserControl()
    {
        InitializeComponent();
        DataContext = new AddWorkMasterProductionUserControlViewModel();
    }

    private void Confirm_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not AddWorkMasterProductionUserControlViewModel viewModel) return;
        if (viewModel.Upload())
        {
            var parent = this.FindAncestorOfType<UserControl>()?.DataContext as WorkUserPageUserControlViewModel;
            parent?.CloseAddWorkMasterUserControl();
        }
    }

    public async Task RefreshDataAsync()
    {
        if (DataContext is not AddWorkMasterProductionUserControlViewModel viewModel) return;
        viewModel.ClearForm();
        await viewModel.LoadListComboBoxAsync();
    }
}