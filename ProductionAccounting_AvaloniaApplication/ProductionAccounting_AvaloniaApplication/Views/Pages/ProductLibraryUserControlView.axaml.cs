using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication.Views.Pages;

public partial class ProductLibraryUserControlView : UserControl
{
    public ProductLibraryUserControlView()
    {
        InitializeComponent();
        DataContext = new ProductLibraryUserControlViewModel();
    }
}