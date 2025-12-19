using System;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class ProductionHistoryViewModel
{
    public long Id { get; set; }
    public DateTime ProductionDate { get; set; }
    public decimal? Shift { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public string OperationName { get; set; } = string.Empty;
    public string OperationUnit { get; set; } = string.Empty;
    public decimal Quantity { get; set; }
    public decimal Amount { get; set; }
    public decimal Tonnage { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public long? ProductId { get; set; }
    public long? OperationId { get; set; }

    public string ShiftDisplay => Shift.HasValue ? $"Смена {Shift.Value}" : "Смена не указана";

    public string StatusColor => Status?.ToLower() switch
    {
        "completed" => "#10B981",
        "issued" => "#3B82F6",
        "pending" => "#F59E0B",
        "cancelled" => "#EF4444",
        _ => "#6B7280"
    };
}
