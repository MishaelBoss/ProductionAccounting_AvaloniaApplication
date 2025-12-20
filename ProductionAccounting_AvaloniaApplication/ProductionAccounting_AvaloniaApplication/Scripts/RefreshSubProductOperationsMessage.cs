namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class RefreshSubProductOperationsMessage(double subProductId)
{
    public double SubProductId { get; } = subProductId;
}
