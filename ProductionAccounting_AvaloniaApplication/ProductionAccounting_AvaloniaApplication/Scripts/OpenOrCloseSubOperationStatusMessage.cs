namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseSubOperationStatusMessage(bool shouldOpen,
    double? subProductId = null,
    double? subProductOperationId = null,
    string? operationName = null,
    string? operationCode = null,
    decimal? operationPrice = null,
    decimal? operationTime = null,
    string? operationDescription = null,
    decimal? operationQuantity = null)
{
    public bool ShouldOpen { get; } = shouldOpen;
    public double SubProductId { get; } = subProductId ?? 0;
    public double SubProductOperationId { get; } = subProductOperationId ?? 0;
    public string OperationName { get; } = operationName ?? string.Empty;
    public string OperationCode { get; } = operationCode ?? string.Empty;
    public decimal OperationPrice { get; } = operationPrice ?? 0m;
    public decimal OperationTime { get; } = operationTime ?? 0m;
    public string OperationDescription { get; } = operationDescription ?? string.Empty;
    public decimal OperationQuantity { get; } = operationQuantity ?? 0m;
}
