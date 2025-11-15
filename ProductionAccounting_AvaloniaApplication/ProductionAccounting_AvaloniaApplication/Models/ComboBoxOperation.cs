using ProductionAccounting_AvaloniaApplication.Services.Interfaces;

namespace ProductionAccounting_AvaloniaApplication.Models;

public class ComboBoxOperation(double id, string title, decimal price, string unit) : IComboBoxOperation
{
    public double Id { get; set; } = id;
    public string Title { get; set; } = title;
    public decimal Price { get; set; } = price;
    public string Unit { get; set; } = unit;

    public override string ToString()
        => Title;
}
