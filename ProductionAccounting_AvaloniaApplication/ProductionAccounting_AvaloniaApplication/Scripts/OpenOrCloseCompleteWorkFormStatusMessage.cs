namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseCompleteWorkFormStatusMessage
{
    public bool ShouldOpen { get; }
    public double AssignmentId { get; }
    public string TaskName { get; }
    public decimal AssignedQuantity { get; }
    public double ProductId { get; }
    public double OperationId { get; }
    public double SubProductOperationId { get; }

    public OpenOrCloseCompleteWorkFormStatusMessage(bool shouldOpen, 
        double? assignmentId = null, 
        string? taskName = null, 
        decimal? assignedQuantity = null, 
        double? productId = null,
        double? operationId = null,
        double? subProductOperationId = null)
    {
        ShouldOpen = shouldOpen;
        AssignmentId = assignmentId ?? 0;
        TaskName = taskName ?? string.Empty;
        AssignedQuantity = assignedQuantity ?? 0;
        ProductId = productId ?? 0;
        OperationId = operationId ?? 0;
        SubProductOperationId = subProductOperationId ?? 0;
    }
}
