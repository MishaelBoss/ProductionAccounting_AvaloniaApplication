using Avalonia.Controls;
using System;
using System.Collections.Generic;

namespace ProductionAccounting_AvaloniaApplication.Scripts;

public static class StackPanelHelper
{
    public static void ClearAndRefreshStackPanel<T>(StackPanel? stackPanel, List<T>? userControls) where T : UserControl
    {
        if (stackPanel == null || userControls == null)
            return;

        try
        {
            userControls.Clear();
            RefreshStackPanelContent(stackPanel, userControls);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.Error,
                "Error updating stack panel",
                ex: ex);
        }
    }

    public static void RefreshStackPanelContent<T>(StackPanel? stackPanel, List<T>? userControls) where T : UserControl
    {
        if (stackPanel == null || userControls == null)
            return;

        try
        {
            stackPanel.Children.Clear();

            foreach (var userControl in userControls)
            {
                stackPanel.Children.Add(userControl);
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.Error,
                "Error updating stack panel",
                ex: ex);
        }
    }
}
