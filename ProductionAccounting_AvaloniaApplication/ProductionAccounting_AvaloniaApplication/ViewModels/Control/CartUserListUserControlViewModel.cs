using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartUserListUserControlViewModel : ViewModelBase
{
    private double _userID = 0;
    public double UserID
    {
        get => _userID;
        set => this.RaiseAndSetIfChanged(ref _userID, value);
    }

    private string _login = string.Empty;
    public string Login
    {
        get => _login;
        set => this.RaiseAndSetIfChanged(ref _login, value);
    }

    private string _userName = string.Empty;
    public string UserName {
        get => _userName;
        set => this.RaiseAndSetIfChanged(ref _userName, value);
    }

    private string _password = string.Empty;
    public string Password { 
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    private string _firstName = string.Empty;
    public string FirstName { 
        get => _firstName;
        set => this.RaiseAndSetIfChanged(ref _firstName, value);
    }

    private string _lastName = string.Empty;
    public string LastName { 
        get => _lastName;
        set => this.RaiseAndSetIfChanged(ref _lastName, value);
    }

    private string _dateJoined = string.Empty;
    public string DateJoined { 
        get => _dateJoined;
        set => this.RaiseAndSetIfChanged(ref _dateJoined, value);
    }

    public bool IsVisibleButtonEdit => ManagerCookie.IsAdministrator;
    public bool IsVisibleButtonDelete => ManagerCookie.IsAdministrator;
}
