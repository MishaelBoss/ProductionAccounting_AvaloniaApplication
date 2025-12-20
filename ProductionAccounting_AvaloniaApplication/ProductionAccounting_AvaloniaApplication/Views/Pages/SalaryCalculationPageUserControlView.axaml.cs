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

        this.AttachedToVisualTree += (s, e) =>
        {
            if (DataContext is SalaryCalculationPageUserControlViewModel vm)
            {
                if (TopLevel.GetTopLevel(this) is TopLevel topLevel)
                {
                    vm.StorageProvider = topLevel.StorageProvider;
                }
            }
        };
    }

    private void OnLoaded(object? sender, RoutedEventArgs e) 
    {
        if (DataContext is not SalaryCalculationPageUserControlViewModel viewModel) return;
        viewModel.SalaryContent = SalaryContent;
    }
}