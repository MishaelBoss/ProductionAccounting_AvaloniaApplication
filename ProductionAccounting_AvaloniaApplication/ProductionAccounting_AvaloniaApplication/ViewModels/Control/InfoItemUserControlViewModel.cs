using Avalonia.Media;
using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class InfoItemUserControlViewModel : ViewModelBase
{
    private string _label = string.Empty;
    public string Label 
    {
        get => _label;
        set => this.RaiseAndSetIfChanged(ref _label, value);
    }

    private string _value = string.Empty;
    public string Value
    {
        get => _value;
        set => this.RaiseAndSetIfChanged(ref _value, value);
    }

    private IBrush _valueColor = Brushes.Black;
    public IBrush ValueColor
    {
        get => _valueColor;
        set => this.RaiseAndSetIfChanged(ref _valueColor, value);
    }

    private int _valueSize;
    public int ValueSize
    {
        get => _valueSize;
        set => this.RaiseAndSetIfChanged(ref _valueSize, value);
    }

    private FontWeight _valueWeight;
    public FontWeight ValueWeight
    {
        get => _valueWeight;
        set => this.RaiseAndSetIfChanged(ref _valueWeight, value);
    }
}
