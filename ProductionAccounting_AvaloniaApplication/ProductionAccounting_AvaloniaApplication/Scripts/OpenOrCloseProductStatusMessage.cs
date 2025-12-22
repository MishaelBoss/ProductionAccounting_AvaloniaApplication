namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseOrderStatusMessage(
    bool shouldOpen,
    double? id = null,
    string? name = null,
    string? counterparties = null,
    string? Description = null,
    decimal? TotalWeight = null,
    decimal? coefficient = null)
{
    public bool ShouldOpen { get; } = shouldOpen;
    public double Id { get; } = id ?? 0;
    public string Name { get; } = name ?? string.Empty;
    public string Counterparties { get; } = counterparties ?? string.Empty;
    public string Description { get; } = Description ?? string.Empty;
    public decimal TotalWeight { get; } = TotalWeight ?? 0;
    public decimal Coefficient { get; } = coefficient ?? 1;
}
