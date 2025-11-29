namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseProductViewStatusMessage
{
    public bool ShouldOpen { get; }
    public double ProductId { get; }

    public OpenOrCloseProductViewStatusMessage(bool shouldOpen, double? productId = null)
    {
        ShouldOpen = shouldOpen;
        ProductId = productId ?? 0;
    }
}
