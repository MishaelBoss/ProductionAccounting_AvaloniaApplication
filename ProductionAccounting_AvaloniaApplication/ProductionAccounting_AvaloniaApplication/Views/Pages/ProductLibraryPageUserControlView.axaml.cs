using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication.Views.Pages;

public partial class ProductLibraryPageUserControlView : UserControl
{
    public ProductLibraryPageUserControlView()
    {
        InitializeComponent();
        DataContext = new ProductLibraryPageUserControlViewModel();
    }
}