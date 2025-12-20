namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseCompleteWorkFormStatusMessage(bool shouldOpen,
    string? taskName = null,
    decimal? plannedQuantity = null,
    double? productId = null,
    double? operationId = null,
    double? subProductOperationId = null,
    double? userId = null)
{
    public bool ShouldOpen { get; } = shouldOpen;
    public string TaskName { get; } = taskName ?? string.Empty;
    public decimal PlannedQuantity { get; } = plannedQuantity ?? 0;
    public double ProductId { get; } = productId ?? 0;
    public double OperationId { get; } = operationId ?? 0;
    public double SubProductOperationId { get; } = subProductOperationId ?? 0;
    public double UserId { get; } = userId ?? 0;
}
