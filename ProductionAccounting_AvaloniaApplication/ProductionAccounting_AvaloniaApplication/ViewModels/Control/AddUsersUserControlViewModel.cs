using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class AddUsersUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
    private string _messageerror = string.Empty;
    public string Messageerror
    {
        get => _messageerror;
        set => this.RaiseAndSetIfChanged(ref _messageerror, value);
    }

    private ObservableCollection<ComboBoxTypeRolsUser> _comboBoxItems = [];
    public ObservableCollection<ComboBoxTypeRolsUser> ComboBoxItems
    {
        get => _comboBoxItems;
        set => this.RaiseAndSetIfChanged(ref _comboBoxItems, value);
    }

    private ComboBoxTypeRolsUser? _selectedComboBoxItem;
    public ComboBoxTypeRolsUser? SelectedComboBoxItem
    {
        get => _selectedComboBoxItem;
        set
        {
            if (_selectedComboBoxItem != value)
            {
                _selectedComboBoxItem = value;
                OnPropertyChanged(nameof(SelectedComboBoxItem));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
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

    private string _firstUsername = string.Empty;
    public string FirstUsername
    {
        get => _firstUsername;
        set
        {
            if (_firstUsername != value)
            {
                _firstUsername = value;
                OnPropertyChanged(nameof(FirstUsername));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
    }

    private string _lastUsername = string.Empty;
    public string LastUsername
    {
        get => _lastUsername;
        set
        {
            if (_lastUsername != value)
            {
                _lastUsername = value;
                OnPropertyChanged(nameof(LastUsername));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
    }

    private string _middle_name = string.Empty;
    public string MiddleName
    {
        get => _middle_name;
        set
        {
            if (_middle_name != value)
            {
                _middle_name = value;
                OnPropertyChanged(nameof(MiddleName));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
    }

    public bool IsActiveConfirmButton
        => SelectedComboBoxItem != null
        && !string.IsNullOrEmpty(Login)
        && !string.IsNullOrEmpty(MiddleName) 
        && !string.IsNullOrEmpty(FirstUsername) 
        && !string.IsNullOrEmpty(LastUsername);

    public async Task LoadListTypeToComboBoxAsync()
    {
        try
        {
            string sql = "SELECT * FROM public.user_type";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        ComboBoxItems.Add(new ComboBoxTypeRolsUser(reader.GetDouble(0), reader.GetString(1)));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);
        }
    }

    public bool Upload() 
    {
        try
        {
            if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(FirstUsername) || string.IsNullOrEmpty(LastUsername) || string.IsNullOrEmpty(MiddleName) || SelectedComboBoxItem == null)
            {
                Messageerror = "Не все поля заполнены";
                return false;
            }

            try
            {
                string sql = "INSERT INTO public.user (login, password, first_name, last_name, middle_name) " +
                             "VALUES (@login, @password, @first_name, @last_name, @middle_name) RETURNING id";

                using (var connection = new NpgsqlConnection(Arguments.connection))
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand(sql, connection)) {
                        try
                        {
                            char[] letters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ".ToCharArray();

                            Random random = new();
                            char randomChar = letters[random.Next(letters.Length)];

                            string randomString = string.Empty;
                            for (int i = 0; i < 10; i++) randomString += letters[random.Next(letters.Length)];

                            command.Parameters.AddWithValue("@login", Login);
                            command.Parameters.AddWithValue("@password", randomString);
                            command.Parameters.AddWithValue("@first_name", FirstUsername);
                            command.Parameters.AddWithValue("@last_name", LastUsername);
                            command.Parameters.AddWithValue("@middle_name", MiddleName);

                            var equipmentId = command.ExecuteScalar();
                            if (equipmentId == null) return false;

                            string sql2 = "INSERT INTO public.user_to_user_type (user_id, user_type_id) VALUES (@user_id, @user_type_id)";
                            using (var commandRel = new NpgsqlCommand(sql2, connection))
                            {
                                commandRel.Parameters.AddWithValue("@user_id", equipmentId);
                                commandRel.Parameters.AddWithValue("@user_type_id", SelectedComboBoxItem?.Id ?? 0);
                                commandRel.ExecuteNonQuery();
                            }

                            if (!string.IsNullOrEmpty(randomString))
                            {
                                return true;
                            }
                            else 
                            {
                                Messageerror = "Ошибка генерации пароля";
                                return false;
                            }

                        }
                        catch (Exception ex)
                        {
                            Loges.LoggingProcess(level: LogLevel.ERROR,
                                ex: ex);
                            return false;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.WARNING,
                    ex: ex);
                Messageerror = "Неизвестная ошибка";
                return false;
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);
            return false;
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
