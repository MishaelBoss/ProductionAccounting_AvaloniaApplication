using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
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

    private string _middleName = string.Empty;
    public string MiddleName {
        get => _middleName;
        set => this.RaiseAndSetIfChanged(ref _middleName, value);
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

    public decimal BaseSalary { get; set; }
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;

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
    public ICommand CopyPasswordCommand
    => new RelayCommand(() => TextCopy.ClipboardService.SetText(Password));

    public ICommand EditCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseUserStatusMessage(true, _userID, Login, FirstName, LastName, MiddleName, BaseSalary, Email, Phone)));

    public ICommand ViewCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseProfileUserStatusMessage(true, _userID)));

    public ICommand DeleteCommand
        => new RelayCommand(() =>
        {
            string[] deleteQueries =
            [
                "DELETE FROM public.timesheet WHERE user_id = @id",
                "DELETE FROM public.user_to_user_type WHERE user_id = @id",
                "DELETE FROM public.user_to_departments WHERE user_id = @id",
                "DELETE FROM public.user_to_position WHERE user_id = @id",
                "DELETE FROM public.production WHERE user_id = @id"
            ];

            var viewModel = new ConfirmDeleteWindowViewModel(UserID, Login, "DELETE FROM public.user WHERE id = @id", () => WeakReferenceMessenger.Default.Send(new RefreshUserListMessage()), deleteQueries);

            var window = new ConfirmDeleteWindow()
            {
                DataContext = viewModel,
            };

            viewModel.SetWindow(window);

            window.Show();
        });


    public static bool IsAdministrator
        => ManagerCookie.IsUserLoggedIn()
        && ManagerCookie.IsAdministrator;

    private async Task ChangeActiveUserAsync() {
        try
        {
            string sqlUserTypes = "UPDATE public.user SET is_active = @is_active WHERE id = @id";

            using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();

            using var command = new NpgsqlCommand(sqlUserTypes, connection);
            command.Parameters.AddWithValue("@id", UserID);
            command.Parameters.AddWithValue("@is_active", IsActive);
            command.ExecuteNonQuery();
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
