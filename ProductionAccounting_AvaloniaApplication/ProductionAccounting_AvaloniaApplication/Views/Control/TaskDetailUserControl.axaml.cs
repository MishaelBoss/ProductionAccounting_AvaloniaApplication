using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication;

public partial class TaskDetailUserControl : UserControl
{
    public TaskDetailUserControl()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e) 
    {
        if (DataContext is not TaskDetailUserControlViewModel viewModel) return;
        viewModel.HomeMainContent = HomeMainContent;
        _ = viewModel.LoadSubProductAsync();
    }

    private void Grid_ActualThemeVariantChanged(object? sender, System.EventArgs e)
    {
    }
}