namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseProductViewStatusMessage(bool shouldOpen, string? name = null, double? productId = null, string? mark = null, decimal? coefficient = null, string? notes = null, string? status = null)
{
    public bool ShouldOpen { get; } = shouldOpen;
    public string Name { get; } = name ?? string.Empty;
    public double ProductId { get; } = productId ?? 0;
    public string Mark { get; } = mark ?? string.Empty;
    public decimal Coefficient { get; } = coefficient ?? 0;
    public string Notes { get; } = notes ?? string.Empty;
    public string Status { get; } = status ?? string.Empty;
}
