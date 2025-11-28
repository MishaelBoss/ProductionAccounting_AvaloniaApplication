namespace ProductionAccounting_AvaloniaApplication.Services.Interfaces;

public interface IComboBoxProduct
{
    public double Id { get; set; }
    public string Title { get; set; }
    public string Article { get; set; }
    public string Mark { get; set; }
    public decimal Coefficient { get; set; }
    public decimal PricePerUnit { get; set; }
    public decimal PricePerKg { get; set; }
    public string Unit { get; set; }
    decimal MaxQuantity { get; set; }
}
