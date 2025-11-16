using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartProductUserControlViewModel : ViewModelBase
{
    private double _productID = 0;
    public double ProductID 
    {
        get => _productID;
        set => this.RaiseAndSetIfChanged(ref _productID, value);
    }

    private string _name = string.Empty;
    public string Name 
    { 
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    private string _article = string.Empty;
    public string Article 
    {
        get => _article;
        set => this.RaiseAndSetIfChanged(ref _article, value);
    }

    private string _pricePerUnit = string.Empty;
    public string PricePerUnit 
    { 
        get => _pricePerUnit;
        set => this.RaiseAndSetIfChanged(ref _pricePerUnit, value);
    }

    private string _unit = string.Empty;
    public string Unit
    {
        get => _unit;
        set => this.RaiseAndSetIfChanged(ref _unit, value);
    }

    private string _coefficient = string.Empty;
    public string Coefficient
    {
        get => _coefficient;
        set => this.RaiseAndSetIfChanged(ref _coefficient, value);
    }

    /*    public List<string> AvailableUnits { get; } = new List<string>
        {
            "кг",
            "еденица"
        };

        private string _selectedUnit = string.Empty;
        public string SelectedUnit
        {
            get => _selectedUnit;
            set => this.RaiseAndSetIfChanged(ref _selectedUnit, value);
        }*/

    public bool IsAdministrator
        => ManagerCookie.IsUserLoggedIn() && ManagerCookie.IsAdministrator;
}
