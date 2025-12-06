using System;

namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseProductViewStatusMessage
{
    public bool ShouldOpen { get; }
    public string Name { get; }
    public double ProductId { get; }
    public string Mark { get; }
    public Int32 Coefficient { get; }
    public string Notes { get; }

    public OpenOrCloseProductViewStatusMessage(bool shouldOpen, string? name = null, double? productId = null, string? mark = null, Int32? coefficient = null, string? notes = null)
    {
        ShouldOpen = shouldOpen;
        Name = name ?? string.Empty;
        ProductId = productId ?? 0;
        Mark = mark ?? string.Empty;
        Coefficient = coefficient ?? 0;
        Notes = notes ?? string.Empty;
    }
}
