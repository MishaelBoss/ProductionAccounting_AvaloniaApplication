using ProductionAccounting_AvaloniaApplication.Services.Interfaces;

namespace ProductionAccounting_AvaloniaApplication.Models;

public class ComboBoxShift() : IComboBoxShift
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Hours { get; set; } = string.Empty;

    public override string ToString()
        => $"{Name} ({Hours})";
}
