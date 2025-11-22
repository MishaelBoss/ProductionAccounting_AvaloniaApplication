using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class WorkUserControl : UserControl
{
    public WorkUserControl()
    {
        InitializeComponent();
        DataContext = new WorkUserControlViewModel();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not WorkUserControlViewModel viewModel) return;
        viewModel.CartTasks = CartTasks;
        await viewModel.LoadTodayAsync();
    }
}