namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseProductStatusMessage
{
    public bool ShouldOpen { get; }
    public double Id { get; }
    public string ProductName { get; }
    public string ProductArticle { get; }
    public string ProductDescription { get; }
    public decimal ProductPricePerUnit { get; }
    public decimal ProductPricePerKg { get; }
    public string ProductMark { get; }
    public decimal ProductCoefficient { get; }

    public OpenOrCloseProductStatusMessage(
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
        ShouldOpen = shouldOpen;
        Id = id ?? 0;
        ProductName = productName ?? string.Empty;
        ProductArticle = productArticle ?? string.Empty;
        ProductDescription = productDescription ?? string.Empty;
        ProductPricePerUnit = productPricePerUnit ?? 1.0m;
        ProductPricePerKg = productPricePerKg ?? 1.0m;
        ProductMark = productMark ?? string.Empty;
        ProductCoefficient = productCoefficient ?? 1.0m;
    }
}
