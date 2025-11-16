using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartOperationUserControlViewModel : ViewModelBase
{
    private double _operationID = 0;
    public double OperationID
    {
        get => _operationID;
        set => this.RaiseAndSetIfChanged(ref _operationID, value);
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    private string _operationCode = string.Empty;
    public string OperationCode
    {
        get => _operationCode;
        set => this.RaiseAndSetIfChanged(ref _operationCode, value);
    }

    private string _price = string.Empty;
    public string Price
    {
        get => _price;
        set => this.RaiseAndSetIfChanged(ref _price, value);
    }

    private string _unit = string.Empty;
    public string Unit
    {
        get => _unit;
        set => this.RaiseAndSetIfChanged(ref _unit, value);
    }

    public bool IsVisibleButtonDelete
        => ManagerCookie.IsUserLoggedIn() 
        && ManagerCookie.IsAdministrator;
}
