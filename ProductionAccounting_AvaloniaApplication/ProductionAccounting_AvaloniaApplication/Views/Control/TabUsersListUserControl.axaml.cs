using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class TabUsersListUserControl : UserControl
{
    public TabUsersListUserControl()
    {
        InitializeComponent();
        DataContext = new TabUsersListUserControlViewModel();
        Loaded += OnLoaded;
    }

    private async void OnLoaded(object? sender, RoutedEventArgs e)
    {
        try
        {
            if (DataContext is not TabUsersListUserControlViewModel viewModel) return;
            viewModel.HomeMainContent = HomeMainContent;
            await viewModel.LoadUsersWithResetAsync();
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Critical, 
                ex: ex);
        }
    }
}