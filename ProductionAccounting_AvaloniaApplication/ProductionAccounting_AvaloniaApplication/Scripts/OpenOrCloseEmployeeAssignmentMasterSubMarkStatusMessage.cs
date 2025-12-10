namespace ProductionAccounting_AvaloniaApplication.Scripts;

public record class OpenOrCloseEmployeeAssignmentMasterSubMarkStatusMessage
{
    public bool ShouldOpen { get; }
    public double ProductId { get; }
    public double SubProductId { get; }

    public OpenOrCloseEmployeeAssignmentMasterSubMarkStatusMessage(bool shouldOpen, double? productId = null, double? subProductId = null)
    {
        ShouldOpen = shouldOpen;
        ProductId = productId ?? 0;
        SubProductId = subProductId ?? 0;
    }
}
