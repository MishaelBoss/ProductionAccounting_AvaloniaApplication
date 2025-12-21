using Avalonia.Controls;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using System;
using static ProductionAccounting_AvaloniaApplication.ViewModels.Control.NotFoundUserControlViewModel;

namespace ProductionAccounting_AvaloniaApplication.Scripts;

public static class ItemNotFoundException
{
    public static void Show(StackPanel? stackPanel, ErrorLevel level) 
    {
        var notFoundUserControl = new NotFoundUserControl { DataContext = new NotFoundUserControlViewModel(level) };

        if (stackPanel is null)
            return;

        try
        {
            stackPanel.Children.Clear();
            stackPanel.Children.Add(notFoundUserControl);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.Error,
                "Error updating stack panel",
                ex: ex);
        }
    }
}
