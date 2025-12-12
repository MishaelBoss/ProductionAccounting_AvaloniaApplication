using System.Windows.Input;
using Avalonia.Media.Imaging;
using ProductionAccounting_AvaloniaApplication.Services.Interfaces;

namespace ProductionAccounting_AvaloniaApplication.Models;

public class DashboardButton(string content, ICommand command, Bitmap iconPath, bool? isVisible = null) : IDashboardButton
{
    public string Content { get; set; } = content;
    public ICommand Command { get; set; } = command;
    public Bitmap IconPath { get; set; } = iconPath;
    public bool IsVisible { get; set; } = isVisible ?? true;
}