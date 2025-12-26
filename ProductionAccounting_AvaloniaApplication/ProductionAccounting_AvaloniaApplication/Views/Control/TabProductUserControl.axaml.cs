using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class TabProductUserControl : UserControl
{
    public TabProductUserControl()
    {
        InitializeComponent();
        DataContext = new TabProductUserControlViewModel();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is not TabProductUserControlViewModel viewModel) return;
            viewModel.HomeMainContent = HomeMainContent;
            await viewModel.LoadProductsWithResetAsync();
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning, 
                ex: ex);
        }
    }
}