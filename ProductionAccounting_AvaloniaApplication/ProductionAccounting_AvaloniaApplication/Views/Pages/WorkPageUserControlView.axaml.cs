using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication.Views.Pages;

public partial class WorkPageUserControlView : UserControl
{
    public WorkPageUserControlView()
    {
        InitializeComponent();
        DataContext = new WorkPageUserControlViewModel();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not WorkPageUserControlViewModel viewModel) return;
        viewModel.WorkContent = WorkContent;
        viewModel.ShowWork();
    }
}