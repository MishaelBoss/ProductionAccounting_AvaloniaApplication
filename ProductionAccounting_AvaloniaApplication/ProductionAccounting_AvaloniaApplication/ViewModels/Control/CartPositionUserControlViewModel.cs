using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartPositionUserControlViewModel : ViewModelBase
{
    private double _id = 0;
    public double Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }

    private string _type = string.Empty;
    public string Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
    }

    public bool IsVisibleButtonDelete
        => ManagerCookie.IsUserLoggedIn()
        && ManagerCookie.IsAdministrator;
}
