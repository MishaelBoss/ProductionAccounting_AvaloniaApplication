namespace ProductionAccounting_AvaloniaApplication.Scripts;

public record class OpenOrCloseEmployeeAssignmentMasterSubMarkStatusMessage
{
    public bool ShouldOpen { get; }
    public double ProductId { get; }

    public OpenOrCloseEmployeeAssignmentMasterSubMarkStatusMessage(bool shouldOpen, double? productId = null)
    {
        ShouldOpen = shouldOpen;
        ProductId = productId ?? 0;
    }
}
