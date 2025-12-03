namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class RefreshSubProductOperationsMessage
{
    public double SubProductId { get; }

    public RefreshSubProductOperationsMessage(double subProductId)
    {
        SubProductId = subProductId;
    }
}
