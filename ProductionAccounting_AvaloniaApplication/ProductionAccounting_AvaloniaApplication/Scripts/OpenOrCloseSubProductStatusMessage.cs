namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseSubProductStatusMessage(bool shouldOpen, double? subProductId = null)
{
    public bool ShouldOpen { get; } = shouldOpen;
    public double SubProductId { get; } = subProductId ?? 0;
}
