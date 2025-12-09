namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseEditSubOperationStatusMessage
{
    public bool ShouldOpen { get; }
    public double SubProductId { get; }

    public OpenOrCloseEditSubOperationStatusMessage(bool shouldOpen, double? subProductId = null)
    {
        ShouldOpen = shouldOpen;
        SubProductId = subProductId ?? 0;
    }
}