using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication.Views.Pages;

public partial class AdminPageUserControlView : UserControl
{
    public AdminPageUserControlView()
    {
        InitializeComponent();
        DataContext = new AdminPageUserControlViewModel();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not AdminPageUserControlViewModel viewModel) return;
        viewModel.Content = Content;
    }

    private void UserControl_ActualThemeVariantChanged(object? sender, System.EventArgs e)
    {
    }
}