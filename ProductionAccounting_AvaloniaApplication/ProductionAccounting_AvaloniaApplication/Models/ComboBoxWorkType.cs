namespace ProductionAccounting_AvaloniaApplication.Models;

public class ComboBoxWorkType (double id, string type, decimal coefficient, string measurement, decimal rate)
{
    public double Id { get; } = id;
    public string Type { get; } = type;
    public decimal Coefficient { get; } = coefficient;
    public string Measurement { get; } = measurement;
    public decimal Rate { get; } = rate;

    public override string ToString()
        => Type;
}
