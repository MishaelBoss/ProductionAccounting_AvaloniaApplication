using Avalonia.Controls;
using Avalonia.Input;
using ProductionAccounting_AvaloniaApplication.ViewModels;

namespace ProductionAccounting_AvaloniaApplication;

public partial class ConfirmDeleteWorkshopDepartmentWindow : Window
{
    public ConfirmDeleteWorkshopDepartmentWindow()
    {
        InitializeComponent();
        DataContext = new ConfirmDeleteWorkshopDepartmentWindowViewModel();
    }

    private void TopBorder(object? sender, PointerPressedEventArgs e)
    {
        BeginMoveDrag(e);
    }
}