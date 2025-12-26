namespace ProductionAccounting_AvaloniaApplication.Models;

public class UserFinalSalarySummary
{
    public double UserId { get; set; }
    public string EmployeeName { get; set; } = "";
    public decimal BaseSalary { get; set; }
    public decimal ProductionAmount { get; set; }
    public decimal TotalGross { get; set; }
    public decimal TaxNDFL { get; set; }
    public decimal NetSalary { get; set; }
}