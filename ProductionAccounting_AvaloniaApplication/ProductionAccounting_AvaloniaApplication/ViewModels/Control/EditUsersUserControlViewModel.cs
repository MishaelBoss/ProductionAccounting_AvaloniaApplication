using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class EditUsersUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
    public EditUsersUserControlViewModel(double userID)
    {
        UserID = userID;
        _ = LoadUserDataAsync();
        _ = LoadListTypeToComboBoxAsync();
    }

    private string _messageerror = string.Empty;
    public string Messageerror
    {
        get => _messageerror;
        set => this.RaiseAndSetIfChanged(ref _messageerror, value);
    }

    private double _userID = 0;
    public double UserID {
        get => _userID;
        set => this.RaiseAndSetIfChanged(ref _userID, value);
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

    private string _middleName = string.Empty;
    public string MiddleName
    {
        get => _middleName;
        set
        {
            if (_middleName != value)
            {
                _middleName = value;
                OnPropertyChanged(nameof(MiddleName));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
    }

    private Decimal _baseSalary = 0;
    public Decimal BaseSalary
    {
        get => _baseSalary;
        set
        {
            if (_baseSalary != value)
            {
                _baseSalary = value;
                OnPropertyChanged(nameof(BaseSalary));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
    }

    public bool IsActiveConfirmButton
        => SelectedComboBoxItem != null
        && !string.IsNullOrEmpty(Login)
        && !string.IsNullOrEmpty(FirstUsername)
        && !string.IsNullOrEmpty(LastUsername) 
        && !string.IsNullOrEmpty(MiddleName)
        && BaseSalary > 0;

    private async Task LoadListTypeToComboBoxAsync()
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

    private async Task LoadUserDataAsync()
    {
        try
        {
            string sql = @"
                        SELECT 
                            u.first_name, 
                            u.last_name, 
                            u.middle_name,
                            u.login, 
                            u.base_salary,
                            ut.id as user_type_id,
                            ut.type_user
                        FROM public.""user"" u
                        LEFT JOIN public.user_to_user_type utu ON u.id = utu.user_id
                        LEFT JOIN public.user_type ut ON utu.user_type_id = ut.id
                        WHERE u.id = @userID";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@userID", UserID);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            MiddleName = reader.GetString(0);
                            FirstUsername = reader.GetString(1);
                            LastUsername = reader.GetString(2);
                            Login = reader.GetString(3);
                            BaseSalary = reader.GetDecimal(4);

                            var userTypeId = reader.GetDouble(5);
                            var userTypeName = reader.GetString(6);

                            SelectedComboBoxItem = ComboBoxItems.FirstOrDefault(x => x.Id == userTypeId);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING, ex: ex);
            Messageerror = "Ошибка загрузки данных пользователя";
        }
    }

    public bool Upload()
    {
        try
        {
            if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(LastUsername) || string.IsNullOrEmpty(FirstUsername) || string.IsNullOrEmpty(MiddleName) || BaseSalary <= 0|| SelectedComboBoxItem == null)
            {
                Messageerror = "Не все поля заполнены";
                return false;
            }

            try
            {
                using (var connection = new NpgsqlConnection(Arguments.connection))
                {
                    connection.Open();

                    string sql = "UPDATE public.\"user\" SET login = @newLogin, middle_name = @newMiddleName, first_name = @newFirstName, last_name = @newLastName, base_salary = @newBaseSalary WHERE id = @userID";
                    using (var command = new NpgsqlCommand(sql, connection))
                    {
                        command.Parameters.AddWithValue("@userID", UserID);
                        command.Parameters.AddWithValue("@newLogin", Login);
                        command.Parameters.AddWithValue("@newFirstName", FirstUsername);
                        command.Parameters.AddWithValue("@newLastName", LastUsername);
                        command.Parameters.AddWithValue("@newMiddleName", MiddleName);
                        command.Parameters.AddWithValue("@newBaseSalary", BaseSalary);
                        command.ExecuteNonQuery();
                    }

                    string deleteSql = "DELETE FROM public.user_to_user_type WHERE user_id = @userID";
                    using (var deleteCommand = new NpgsqlCommand(deleteSql, connection))
                    {
                        deleteCommand.Parameters.AddWithValue("@userID", UserID);
                        deleteCommand.ExecuteNonQuery();
                    }

                    string insertSql = "INSERT INTO public.user_to_user_type (user_id, user_type_id) VALUES (@userID, @userTypeID)";
                    using (var insertCommand = new NpgsqlCommand(insertSql, connection))
                    {
                        insertCommand.Parameters.AddWithValue("@userID", UserID);
                        insertCommand.Parameters.AddWithValue("@userTypeID", SelectedComboBoxItem.Id);
                        insertCommand.ExecuteNonQuery();
                    }

                    return true;
                }
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.WARNING, ex: ex);
                Messageerror = $"Ошибка обновления: {ex.Message}";
                return false;
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING, ex: ex);
            Messageerror = "Неизвестная ошибка";
            return false;
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
