using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartUserListUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
    private double _userId;
    [UsedImplicitly]
    public double UserId
    {
        get => _userId;
        set => this.RaiseAndSetIfChanged(ref _userId, value);
    }

    private string _login = string.Empty;
    [UsedImplicitly]
    public string Login
    {
        get => _login;
        set => this.RaiseAndSetIfChanged(ref _login, value);
    }

    private string _middleName = string.Empty;
    [UsedImplicitly]
    public string MiddleName {
        get => _middleName;
        set => this.RaiseAndSetIfChanged(ref _middleName, value);
    }

    private string _password = string.Empty;
    [UsedImplicitly]
    public string Password { 
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    private string _firstName = string.Empty;
    [UsedImplicitly]
    public string FirstName { 
        get => _firstName;
        set => this.RaiseAndSetIfChanged(ref _firstName, value);
    }

    private string _lastName = string.Empty;
    [UsedImplicitly]
    public string LastName { 
        get => _lastName;
        set => this.RaiseAndSetIfChanged(ref _lastName, value);
    }

    [UsedImplicitly]
    public decimal BaseSalary { get; set; }
    
    [UsedImplicitly]
    public string Email { get; set; } = string.Empty;
    
    [UsedImplicitly]
    public string Phone { get; set; } = string.Empty;

    private string _dateJoined = string.Empty;
    [UsedImplicitly]
    public string DateJoined { 
        get => _dateJoined;
        set => this.RaiseAndSetIfChanged(ref _dateJoined, value);
    }

    private bool? _isActive;
    [UsedImplicitly]
    public bool IsActive 
    {
        get => _isActive ?? false;
        set
        {
            this.RaiseAndSetIfChanged(ref _isActive, value);
            _ = ChangeActiveUserAsync();
        }
    }
    public ICommand CopyPasswordCommand
    => new RelayCommand(() => TextCopy.ClipboardService.SetText(Password));

    public ICommand EditCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseUserStatusMessage(true, _userId, Login, FirstName, LastName, MiddleName, BaseSalary, Email, Phone)));

    public ICommand ViewCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseProfileUserStatusMessage(true, _userId)));

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

            var viewModel = new ConfirmDeleteWindowViewModel(UserId, Login, "DELETE FROM public.user WHERE id = @id", () => WeakReferenceMessenger.Default.Send(new RefreshUserListMessage()), deleteQueries);

            var window = new ConfirmDeleteWindow()
            {
                DataContext = viewModel,
            };

            viewModel.SetWindow(window);

            window.Show();
        });


    public static bool IsAdministrator
        => ManagerCookie.IsUserLoggedIn();

    private async Task ChangeActiveUserAsync() {
        try
        {
            const string sqlUserTypes = "UPDATE public.user SET is_active = @is_active WHERE id = @id";

            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sqlUserTypes, connection);
            command.Parameters.AddWithValue("@id", UserId);
            command.Parameters.AddWithValue("@is_active", IsActive);
            command.ExecuteNonQuery();
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning, ex: ex);
        }
    }
}
