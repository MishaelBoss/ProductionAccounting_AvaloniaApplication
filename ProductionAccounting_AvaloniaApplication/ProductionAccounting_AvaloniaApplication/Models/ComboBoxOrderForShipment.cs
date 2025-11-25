using ProductionAccounting_AvaloniaApplication.Services.Interfaces;

namespace ProductionAccounting_AvaloniaApplication.Models;

public class ComboBoxOrderForShipment(double id, string orderNumber, string customerName, decimal producedWeight, decimal producedQuantity) : IComboBoxOrderForShipment
{
    public double Id { get; set; } = id;
    public string OrderNumber { get; set; } = orderNumber;
    public string CustomerName { get; set; } = customerName;
    public decimal ProducedWeight { get; set; } = producedWeight;
    public decimal ProducedQuantity { get; set; } = producedQuantity;

    public string DisplayText
        => $"{OrderNumber} — {CustomerName}";

    public string InfoText 
        => $"Готово: {ProducedWeight:F3} т | {ProducedQuantity:F2} шт.";
}
