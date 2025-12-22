using System;
using System.Windows.Input;
using Avalonia.Media.Imaging;
using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class DashboardButtonViewModel(string content, ICommand command, Bitmap iconPath, Func<bool>? isVisibleFunc = null) : ViewModelBase
{
    private string _content = content;
    public string Content 
    { 
        get => _content; 
        set => this.RaiseAndSetIfChanged(ref _content, value); 
    }
    
    public ICommand Command { get; } = command;
    public Bitmap IconPath { get; } = iconPath;
    private Func<bool> IsVisibleFunc { get; } = isVisibleFunc ?? (() => true);
    
    public bool IsVisible => IsVisibleFunc();
    
    public void UpdateVisibility()
    {
        this.RaisePropertyChanged(nameof(IsVisible));
    }
}