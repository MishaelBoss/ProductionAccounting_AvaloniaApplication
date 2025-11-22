using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication.Views.Pages;

public partial class TimesheetPageUserControlView : UserControl
{
    public TimesheetPageUserControlView()
    {
        InitializeComponent();
        DataContext = new TimesheetPageUserControlViewModel();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e) 
    {
        if (DataContext is not TimesheetPageUserControlViewModel viewModel) return;
        viewModel.CartTimesheet = CartTimesheet;
        await viewModel.LoadTimesheetAsync();
    }
}