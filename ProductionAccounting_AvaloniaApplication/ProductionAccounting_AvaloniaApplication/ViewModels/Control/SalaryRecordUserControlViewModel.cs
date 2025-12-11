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
        set
        {
            this.RaiseAndSetIfChanged(ref _isSummary, value);
            UpdateDisplayProperties();
        }
    }

    private bool _isTotal;
    public bool IsTotal
    {
        get => _isTotal;
        set
        {
            this.RaiseAndSetIfChanged(ref _isTotal, value);
            UpdateDisplayProperties();
        }
    }

    public bool ShowDetails => !IsSummary && !IsTotal;
    public bool ShowSummary => IsSummary && !IsTotal;
    public bool ShowTotal => IsTotal;

    private string _unit = "шт";
    public string Unit
    {
        get => _unit;
        set => this.RaiseAndSetIfChanged(ref _unit, value);
    }

    private decimal _rate;
    public decimal Rate
    {
        get => _rate;
        set => this.RaiseAndSetIfChanged(ref _rate, value);
    }

    private decimal _coefficient = 1.0m;
    public decimal Coefficient
    {
        get => _coefficient;
        set => this.RaiseAndSetIfChanged(ref _coefficient, value);
    }

    private bool _useTonnage;
    public bool UseTonnage
    {
        get => _useTonnage;
        set => this.RaiseAndSetIfChanged(ref _useTonnage, value);
    }

     public void UpdateDisplayProperties()
    {
        this.RaisePropertyChanged(nameof(ShowDetails));
        this.RaisePropertyChanged(nameof(ShowSummary));
        this.RaisePropertyChanged(nameof(ShowTotal));
    }

    public bool ShowTonnage => UseTonnage && Tonnage > 0;
}
