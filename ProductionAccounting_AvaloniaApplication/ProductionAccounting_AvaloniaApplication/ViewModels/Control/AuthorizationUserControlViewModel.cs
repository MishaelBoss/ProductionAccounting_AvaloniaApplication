using Npgsql;
using ProductionAccounting_AvaloniaApplication.Script;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.IO;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class AuthorizationUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
    private string _messageerror = string.Empty;
    public string Messageerror
    {
        get => _messageerror;
        set => this.RaiseAndSetIfChanged(ref _messageerror, value);
    }

    private string _fullName = string.Empty;
    public string FullName
    {
        get => _fullName;
        set 
        {
            if (_fullName != value) 
            {
                _fullName = value;
                OnPropertyChanged(nameof(FullName));
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
        => !string.IsNullOrEmpty(FullName)
        && !string.IsNullOrEmpty(Password);

    public bool Authorization()
    {
        try
        {
            if (string.IsNullOrEmpty(FullName) || string.IsNullOrEmpty(Password))
            {
                Messageerror = "Логин и пароль обязательны";
                return false;
            }

            try
            {
                string[] words = FullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (words.Length < 3)
                {
                    Messageerror = "Введите Фамилию Имя Отчество полностью";
                    return false;
                }

                string lastName = words[0];
                string firstName = words[1];
                string middleName = words[2];

                string query = @"SELECT id, name, password, first_name, last_name FROM public.""user"" WHERE name = @name AND first_name = @first_name AND last_name = @last_name";

                using (var connection = new NpgsqlConnection(Arguments.connection))
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@name", middleName);
                        command.Parameters.AddWithValue("@first_name", firstName);
                        command.Parameters.AddWithValue("@last_name", lastName);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                double dbid = reader.GetDouble(0);
                                string dbusername = reader.GetString(1);
                                string dbpassword = reader.GetString(2);

                                if (dbpassword == Password)
                                {
                                    if (!Directory.Exists(Paths.SharedFolder))
                                        Directory.CreateDirectory(Paths.SharedFolder);

                                    ManagerCookie.SaveLoginCookie(dbid, dbusername, Guid.NewGuid().ToString(), DateTime.Now.AddDays(7), Paths.SharedFolder);

                                    return true;
                                }
                                else
                                {
                                    Messageerror = "Пароль неверный";
                                    return false;
                                }
                            }
                            else
                            {
                                Messageerror = "Пользователь не найден";
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.ERROR,
                    ex: ex,
                    message: "Error connecting to database");
                Messageerror = "Ошибка подключения к базе данных";
                return false;
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.ERROR,
                ex: ex,
                message: "Unexpected error in authorization");
            Messageerror = "Неожиданная ошибка";
            return false;
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
