namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseCompleteWorkFormStatusMessage
{
    public bool ShouldOpen { get; }
    public string TaskName { get; }
    public decimal PlannedQuantity { get; }
    public double ProductId { get; }
    public double OperationId { get; }
    public double SubProductOperationId { get; }
    public double UserId { get; }

    public OpenOrCloseCompleteWorkFormStatusMessage(bool shouldOpen, 
        string? taskName = null, 
        decimal? plannedQuantity = null, 
        double? productId = null,
        double? operationId = null,
        double? subProductOperationId = null,
        double? userId = null)
    {
        ShouldOpen = shouldOpen;
        TaskName = taskName ?? string.Empty;
        PlannedQuantity = plannedQuantity ?? 0;
        ProductId = productId ?? 0;
        OperationId = operationId ?? 0;
        SubProductOperationId = subProductOperationId ?? 0;
        UserId = userId ?? 0;
    }
}
