using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class TabWorkshopDepartmentUserControl : UserControl
{
    public TabWorkshopDepartmentUserControl()
    {
        InitializeComponent();
        DataContext = new TabWorkshopDepartmentUserControlViewModel();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not TabWorkshopDepartmentUserControlViewModel viewModel) return;
        viewModel.HomeMainContent = HomeMainContent;
        viewModel.GetList();
    }
}