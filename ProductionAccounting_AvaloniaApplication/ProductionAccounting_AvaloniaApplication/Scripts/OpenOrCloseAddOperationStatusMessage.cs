namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseAddOperationStatusMessage
{
    public bool ShouldOpen { get; }
    public double SubProductId { get; }

    public OpenOrCloseAddOperationStatusMessage(bool shouldOpen, double? subProductId = null)
    {
        ShouldOpen = shouldOpen;
        SubProductId = subProductId ?? 0;
    }
}
