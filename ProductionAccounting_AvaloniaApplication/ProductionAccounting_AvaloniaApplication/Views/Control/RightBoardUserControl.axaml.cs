using Avalonia.Controls;
using Avalonia.VisualTree;
using ProductionAccounting_AvaloniaApplication.ViewModels;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using System;

namespace ProductionAccounting_AvaloniaApplication.Views.Control;

public partial class RightBoardUserControl : UserControl
{
    public RightBoardUserControl()
    {
        InitializeComponent();
        Loaded += OnLoaded;
    }

    private void OnLoaded(object? sender, EventArgs e)
    {
        Loaded -= OnLoaded;

        if (DataContext == null)
        {
            var mainWindow = this.FindAncestorOfType<Window>();
            if (mainWindow?.DataContext is MainWindowViewModel mainViewModel)
            {
                DataContext = new RightBoardUserControlViewModel(mainViewModel);
            }
        }
    }
}