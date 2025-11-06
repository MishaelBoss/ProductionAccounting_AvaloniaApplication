using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication;

public partial class ProductLibraryUserControl : UserControl
{
    public ProductLibraryUserControl()
    {
        InitializeComponent();
        DataContext = new ProductLibraryUserControlViewModel();
    }
}