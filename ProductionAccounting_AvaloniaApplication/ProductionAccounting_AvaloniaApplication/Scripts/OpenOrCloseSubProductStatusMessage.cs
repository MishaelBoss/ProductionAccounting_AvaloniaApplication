namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseSubProductStatusMessage
{
    public bool ShouldOpen { get; }
    public double SubProductId { get; }

    public OpenOrCloseSubProductStatusMessage(bool shouldOpen, double? subProductId = null)
    {
        ShouldOpen = shouldOpen;
        SubProductId = subProductId ?? 0;
    }
}
