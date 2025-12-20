using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class WorkMasterUserControl : UserControl
{
    public WorkMasterUserControl()
    {
        InitializeComponent();
        DataContext = new WorkMasterUserControlViewModel();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not WorkMasterUserControlViewModel viewModel) return;
        viewModel.CartTasks = CartTasks;
        await viewModel.LoadTasksAsync();
    }
}