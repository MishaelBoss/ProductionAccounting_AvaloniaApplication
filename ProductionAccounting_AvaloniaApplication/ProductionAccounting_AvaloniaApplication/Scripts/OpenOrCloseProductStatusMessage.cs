namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseProductStatusMessage(
    bool shouldOpen,
    double? id = null,
    string? productName = null,
    string? productArticle = null,
    string? productDescription = null,
    decimal? productPricePerUnit = 0,
    decimal? productPricePerKg = null,
    string? productMark = null,
    decimal? productCoefficient = null)
{
    public bool ShouldOpen { get; } = shouldOpen;
    public double Id { get; } = id ?? 0;
    public string ProductName { get; } = productName ?? string.Empty;
    public string ProductArticle { get; } = productArticle ?? string.Empty;
    public string ProductDescription { get; } = productDescription ?? string.Empty;
    public decimal ProductPricePerUnit { get; } = productPricePerUnit ?? 1.0m;
    public decimal ProductPricePerKg { get; } = productPricePerKg ?? 1.0m;
    public string ProductMark { get; } = productMark ?? string.Empty;
    public decimal ProductCoefficient { get; } = productCoefficient ?? 1.0m;
}
