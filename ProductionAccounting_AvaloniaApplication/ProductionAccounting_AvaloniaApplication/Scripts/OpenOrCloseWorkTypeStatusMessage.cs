namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseWorkTypeStatusMessage(bool shouldOpen, double? id = null, string? type = null, decimal? coefficient = null, string? measurement = null, decimal? rate = null)
{
    public bool ShouldOpen { get; } = shouldOpen;
    public double Id { get; } = id ?? 0;
    public string Type { get; } = type ?? string.Empty;
    public decimal Coefficient { get; } = coefficient ?? 0m;
    public string Measurement { get; } = measurement ?? string.Empty;
    public decimal Rate { get; } = rate ?? 0m;
}
