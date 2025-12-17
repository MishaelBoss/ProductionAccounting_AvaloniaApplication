using ProductionAccounting_AvaloniaApplication.Services.Interfaces;

namespace ProductionAccounting_AvaloniaApplication.Models;

public class ComboBoxLocalization(string name, string fileName) : IComboBoxLocalization
{
    public string Name { get; set; } = name;
    public string FileName { get; set; } = fileName;
    public override string ToString()
        => Name;
}