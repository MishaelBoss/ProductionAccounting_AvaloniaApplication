using System;
using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class SalaryRecordUserControlViewModel : ViewModelBase
{
    private string _employeeName = "";
    public string EmployeeName
    {
        get => _employeeName;
        set => this.RaiseAndSetIfChanged(ref _employeeName, value);
    }
    
    private string _operationName = "";
    public string OperationName
    {
        get => _operationName;
        set => this.RaiseAndSetIfChanged(ref _operationName, value);
    }
    
    private DateTime _productionDate;
    public DateTime ProductionDate
    {
        get => _productionDate;
        set => this.RaiseAndSetIfChanged(ref _productionDate, value);
    }
    
    private decimal _quantity;
    public decimal Quantity
    {
        get => _quantity;
        set => this.RaiseAndSetIfChanged(ref _quantity, value);
    }
    
    private decimal _tonnage;
    public decimal Tonnage
    {
        get => _tonnage;
        set => this.RaiseAndSetIfChanged(ref _tonnage, value);
    }
    
    private decimal _amount;
    public decimal Amount
    {
        get => _amount;
        set => this.RaiseAndSetIfChanged(ref _amount, value);
    }
    
    private string _calculationFormula = "";
    public string CalculationFormula
    {
        get => _calculationFormula;
        set => this.RaiseAndSetIfChanged(ref _calculationFormula, value);
    }
    
    private bool _isSummary;
    public bool IsSummary
    {
        get => _isSummary;
        set => this.RaiseAndSetIfChanged(ref _isSummary, value);
    }
    
    private bool _isTotal;
    public bool IsTotal
    {
        get => _isTotal;
        set => this.RaiseAndSetIfChanged(ref _isTotal, value);
    }
    
    public bool ShowDetails => !IsSummary && !IsTotal;
    public bool ShowSummary => IsSummary;
    public bool ShowTotal => IsTotal;
}
