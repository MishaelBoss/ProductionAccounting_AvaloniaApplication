using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication.Views.Pages;

public partial class WorkUserPageUserControlView : UserControl
{
    public WorkUserPageUserControlView()
    {
        InitializeComponent();
        DataContext = new WorkUserPageUserControlViewModel();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not WorkUserPageUserControlViewModel viewModel) return;
        viewModel.ContentCenter = ContentCenter;
    }

    private void Border_ActualThemeVariantChanged(object? sender, System.EventArgs e)
    {
    }
}