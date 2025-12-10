using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication.Views.Pages;

public partial class SalaryCalculationPageUserControlView : UserControl
{
    public SalaryCalculationPageUserControlView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e) 
    {
        if (DataContext is not SalaryCalculationPageUserControlViewModel viewModel) return;
        viewModel.SalaryContent = SalaryContent;
    }
}