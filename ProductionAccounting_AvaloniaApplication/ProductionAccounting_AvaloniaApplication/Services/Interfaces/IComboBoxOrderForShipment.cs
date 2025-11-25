namespace ProductionAccounting_AvaloniaApplication.Services.Interfaces;

public interface IComboBoxOrderForShipment
{
    public double Id { get; set; }
    public string OrderNumber { get; set; }
    public string CustomerName { get; set; }
    public decimal ProducedWeight { get; set; }
    public decimal ProducedQuantity { get; set; }
}