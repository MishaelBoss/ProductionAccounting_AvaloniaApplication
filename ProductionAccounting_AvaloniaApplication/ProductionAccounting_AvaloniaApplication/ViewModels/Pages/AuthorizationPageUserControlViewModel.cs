using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class AuthorizationPageUserControlViewModel : ViewModelBase
{
    private string _messageerror = string.Empty;
    public string Messageerror
    {
        get => _messageerror;
        set => this.RaiseAndSetIfChanged(ref _messageerror, value);
    }

    private string _login = string.Empty;
    [UsedImplicitly]
    public string Login
    {
        get => _login;
        set
        {
            this.RaiseAndSetIfChanged(ref _login, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    private string _password = string.Empty;
    [UsedImplicitly]
    public string Password
    {
        get => _password;
        set
        {
            this.RaiseAndSetIfChanged(ref _password, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    private Bitmap? _eyeIcon;
    public Bitmap? EyeIcon
    {
        get => _eyeIcon;
        set => this.RaiseAndSetIfChanged(ref _eyeIcon, value);
    }

    private bool _isPasswordVisible;
    [UsedImplicitly]
    public bool IsPasswordVisible
    {
        get => _isPasswordVisible;
        set => this.RaiseAndSetIfChanged(ref _isPasswordVisible, value);
    }

    public AuthorizationPageUserControlViewModel()
    {
        UpdateEyeIcon();
    }

    public ICommand TogglePasswordVisibilityCommand
        => new RelayCommand(() => { IsPasswordVisible = !IsPasswordVisible; UpdateEyeIcon(); });

    public ICommand ConfirmCommand
        => new RelayCommand(async void () =>
        {
            try
            {
                await Authorization();
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.Critical,
                    ex: ex,
                    message: "Failed to log in");
            }
        });

    public bool IsActiveConfirmButton
        => !string.IsNullOrEmpty(Login)
        && !string.IsNullOrEmpty(Password);

    private void UpdateEyeIcon()
    {
        var uri = new Uri(IsPasswordVisible ? "avares://ProductionAccounting_AvaloniaApplication/Assets/eye-show-64.png" : "avares://ProductionAccounting_AvaloniaApplication/Assets/eye-hide-64.png");

        using var stream = AssetLoader.Open(uri);
        EyeIcon = new Bitmap(stream);
    }

    private async Task Authorization()
    {
        try
        {
            if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(Password))
            {
                Messageerror = "Логин и пароль обязательны";
                return;
            }

            try
            {
                const string query = @"SELECT id, login, last_name, first_name, middle_name, password FROM public.""user"" WHERE login = @login";

                await using var connection = new NpgsqlConnection(Arguments.Connection);
                await connection.OpenAsync();
                await using var command = new NpgsqlCommand(query, connection);
                command.Parameters.AddWithValue("@login", Login);

                await using var reader = command.ExecuteReader();
                if (reader.Read())
                {
                    var id = reader.GetDouble(0);
                    var login = reader.GetString(1);
                    var password = reader.GetString(5);

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
                Loges.LoggingProcess(level: LogLevel.Error,
                    ex: ex,
                    message: "Error connecting to database");
                Messageerror = "Ошибка подключения к базе данных";
            }
        }
        catch (PostgresException ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex,
                message: "Unexpected error in authorization");
            Messageerror = "Неожиданная ошибка";
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Error,
                ex: ex,
                message: "Unexpected error in authorization");
            Messageerror = "Неожиданная ошибка";
        }
    }

    private void ClearForm()
    {
        Messageerror = string.Empty;
        Login = string.Empty;
        Password = string.Empty;
    }
}