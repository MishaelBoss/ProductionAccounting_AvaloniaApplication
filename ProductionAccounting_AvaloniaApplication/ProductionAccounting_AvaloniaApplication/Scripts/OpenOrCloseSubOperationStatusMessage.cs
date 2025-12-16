namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class OpenOrCloseSubOperationStatusMessage
{
    public bool ShouldOpen { get; }
    public double SubProductId { get; }
    public double SubProductOperationId { get; }
    public string OperationName { get; }
    public string OperationCode { get; }
    public decimal OperationPrice { get; }
    public decimal OperationTime { get; }
    public string OperationDescription { get; }
    public decimal OperationQuantity { get; }

    public OpenOrCloseSubOperationStatusMessage(bool shouldOpen, 
        double? subProductId = null, 
        double? subProductOperationId = null,
        string? operationName = null, 
        string? operationCode = null, 
        decimal? operationPrice = null, 
        decimal? operationTime = null, 
        string? operationDescription = null, 
        decimal? operationQuantity = null)
    {
        ShouldOpen = shouldOpen;
        SubProductId = subProductId ?? 0;
        SubProductOperationId = subProductOperationId ?? 0;
        OperationName = operationName ?? string.Empty;
        OperationCode = operationCode ?? string.Empty;
        OperationPrice = operationPrice ?? 0m;
        OperationTime = operationTime ?? 0m;
        OperationDescription = operationDescription ?? string.Empty;
        OperationQuantity = operationQuantity ?? 0m;
    }
}
