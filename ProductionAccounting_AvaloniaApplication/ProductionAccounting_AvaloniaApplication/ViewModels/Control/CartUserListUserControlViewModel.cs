using CommunityToolkit.Mvvm.Input;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartUserListUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
    public ICommand CopyPasswordCommand
        => new RelayCommand(() => TextCopy.ClipboardService.SetText(Password));

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

    private bool? _isActive;
    public bool IsActive 
    {
        get => _isActive ?? false;
        set
        {
            if (_isActive != value) 
            { 
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
                _ = ChangeActiveUserAsync();
            }
        }
    }

    public bool IsVisibleToggleButtonActiveUser
        => ManagerCookie.IsUserLoggedIn() && ManagerCookie.IsAdministrator;

    public bool IsVisibleButtonEdit 
        => ManagerCookie.IsUserLoggedIn() && ManagerCookie.IsAdministrator;

    public bool IsVisibleButtonDelete
        => ManagerCookie.IsUserLoggedIn() && ManagerCookie.IsAdministrator;

    private async Task ChangeActiveUserAsync() {
        try
        {
            string sqlUserTypes = "UPDATE public.user SET is_active = @is_active WHERE id = @id";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(sqlUserTypes, connection))
                {
                    command.Parameters.AddWithValue("@id", UserID);
                    command.Parameters.AddWithValue("@is_active", IsActive);
                    command.ExecuteNonQuery();
                }
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING, ex: ex);
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
