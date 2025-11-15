using ProductionAccounting_AvaloniaApplication.Services.Interfaces;

namespace ProductionAccounting_AvaloniaApplication.Models;

public class ComboBoxProduct(double id, string title, string article, string mark, decimal coefficient, decimal pricePerUnit, decimal pricePerKg, string unit) : IComboBoxProduct
{
    public double Id { get; set; } = id;
    public string Title { get; set; } = title;
    public string Article { get; set; } = article;
    public string Mark { get; set; } = mark;
    //public string WorkType { get; set; } = workType;
    public decimal Coefficient { get; set; } = coefficient;
    public decimal PricePerUnit { get; set; } = pricePerUnit;
    public decimal PricePerKg { get; set; } = pricePerKg;
    public string Unit { get; set; } = unit;

    public decimal MaxQuantity { get; set; } = 0;

    public override string ToString()
        => Title;
}
