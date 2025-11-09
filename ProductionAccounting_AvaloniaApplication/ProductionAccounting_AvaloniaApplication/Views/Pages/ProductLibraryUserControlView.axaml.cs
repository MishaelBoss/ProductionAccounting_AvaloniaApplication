using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

namespace ProductionAccounting_AvaloniaApplication.Views.Pages;

public partial class ProductLibraryUserControlView : UserControl
{
    public ProductLibraryUserControlView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        DataContext = new ProductLibraryUserControlViewModel();
    }

    private void OnLoaded(object? sender, RoutedEventArgs e) 
    {
        if (DataContext is not ProductLibraryUserControlViewModel viewModel) return;
        viewModel.MainContent = MainContent;
        viewModel.GetListProduct();
    }

    private void DownloadCloud_Click(object? sender, RoutedEventArgs e) 
    {
    }

    private void ReturnList_Click(object? sender, RoutedEventArgs e)
    {
    }
}