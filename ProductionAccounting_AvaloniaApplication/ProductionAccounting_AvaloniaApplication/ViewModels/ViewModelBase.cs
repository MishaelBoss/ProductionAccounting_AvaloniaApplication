using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels;

public class ViewModelBase : ReactiveObject
{
    public bool IsAdministrator
        => ManagerCookie.IsUserLoggedIn() && ManagerCookie.IsAdministrator;
}
