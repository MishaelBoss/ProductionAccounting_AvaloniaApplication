using System.Windows.Input;
using Avalonia.Media.Imaging;

namespace ProductionAccounting_AvaloniaApplication.Services.Interfaces;

public interface IDashboardButton
{
    public string Content { get; set; }
    public ICommand Command { get; set; }
    public Bitmap IconPath { get; set; }
    public bool IsVisible { get; set; }
}