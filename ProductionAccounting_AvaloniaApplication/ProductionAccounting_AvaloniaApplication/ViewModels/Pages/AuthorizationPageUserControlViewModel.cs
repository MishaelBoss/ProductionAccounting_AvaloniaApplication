using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class AuthorizationPageUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
    private ICommand ConfirmCommand
        => new RelayCommand(async () => await Authorization());

    private string _messageerror = string.Empty;
    public string Messageerror
    {
        get => _messageerror;
        set => this.RaiseAndSetIfChanged(ref _messageerror, value);
    }

    private string _login = string.Empty;
    public string Login
    {
        get => _login;
        set
        {
            if (_login != value)
            {
                _login = value;
                OnPropertyChanged(nameof(Login));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set
        {
            if (_password != value)
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
    }

    public bool IsActiveConfirmButton
        => !string.IsNullOrEmpty(Login)
        && !string.IsNullOrEmpty(Password);

    public async Task Authorization()
    {
        try
        {
            if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(Password))
            {
                Messageerror = "Логин и пароль обязательны";
            }

            try
            {
                string query = @"SELECT id, login, last_name, first_name, middle_name, password FROM public.""user"" WHERE login = @login";

                using var connection = new NpgsqlConnection(Arguments.Connection);
                connection.Open();
                using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@login", Login);

                using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    double id = reader.GetDouble(0);
                    string login = reader.GetString(1);
                    string password = reader.GetString(5);

                    if (password == Password)
                    {
                        if (!Directory.Exists(Paths.SharedFolder))
                            Directory.CreateDirectory(Paths.SharedFolder);

                        ManagerCookie.SaveLoginCookie(id, login, Guid.NewGuid().ToString(), DateTime.Now.AddDays(7), Paths.SharedFolder);

                        StrongReferenceMessenger.Default.Send(new UserAuthenticationChangedMessage());

                        ClearForm();
                    }
                    else
                    {
                        Messageerror = "Пароль неверный";
                    }
                }
                else
                {
                    Messageerror = "Пользователь не найден";
                }
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.ERROR,
                    ex: ex,
                    message: "Error connecting to database");
                Messageerror = "Ошибка подключения к базе данных";
            }
        }
        catch (PostgresException ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex,
                message: "Unexpected error in authorization");
            Messageerror = "Неожиданная ошибка";
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.ERROR,
                ex: ex,
                message: "Unexpected error in authorization");
            Messageerror = "Неожиданная ошибка";
        }
    }

    public void ClearForm()
    {
        Messageerror = string.Empty;
        Login = string.Empty;
        Password = string.Empty;
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
