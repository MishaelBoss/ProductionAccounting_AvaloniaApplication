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

    private ObservableCollection<ComboBoxTypeDepartmentUser> _comboBoxItemsDepartments = [];
    public ObservableCollection<ComboBoxTypeDepartmentUser> ComboBoxItemsDepartments
    {
        get => _comboBoxItemsDepartments;
        set => this.RaiseAndSetIfChanged(ref _comboBoxItemsDepartments, value);
    }

    private ComboBoxTypeDepartmentUser? _selectedComboBoxItemDepartment;
    public ComboBoxTypeDepartmentUser? SelectedComboBoxItemDepartment
    {
        get => _selectedComboBoxItemDepartment;
        set
        {
            if (_selectedComboBoxItemDepartment != value)
            {
                _selectedComboBoxItemDepartment = value;
                OnPropertyChanged(nameof(SelectedComboBoxItemDepartment));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
            }
        }
    }

    private ObservableCollection<ComboBoxTypePositionUser> _comboBoxItemsPositions = [];
    public ObservableCollection<ComboBoxTypePositionUser> ComboBoxItemsPositions
    {
        get => _comboBoxItemsPositions;
        set => this.RaiseAndSetIfChanged(ref _comboBoxItemsPositions, value);
    }

    private ComboBoxTypePositionUser? _selectedComboBoxItemPosition;
    public ComboBoxTypePositionUser? SelectedComboBoxItemPosition
    {
        get => _selectedComboBoxItemPosition;
        set
        {
            if (_selectedComboBoxItemPosition != value)
            {
                _selectedComboBoxItemPosition = value;
                OnPropertyChanged(nameof(SelectedComboBoxItemPosition));
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
        && SelectedComboBoxItemDepartment != null
        && SelectedComboBoxItemPosition != null
        && !string.IsNullOrEmpty(Login)
        && !string.IsNullOrEmpty(FirstUsername)
        && !string.IsNullOrEmpty(LastUsername) 
        && !string.IsNullOrEmpty(MiddleName)
        && BaseSalary > 0;

    public async Task LoadListTypeToComboBoxAsync()
    {
        try
        {
            string sqlUserTypes = "SELECT id, type_user FROM public.user_type";
            string sqlDepartments = "SELECT id, type FROM public.departments";
            string sqlPositions = "SELECT id, type FROM public.positions";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(sqlUserTypes, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        ComboBoxItems.Add(new ComboBoxTypeRolsUser(reader.GetDouble(0),reader.GetString(1)));
                    }
                }

                using (var command = new NpgsqlCommand(sqlDepartments, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        ComboBoxItemsDepartments.Add(new ComboBoxTypeDepartmentUser(reader.GetDouble(0),reader.GetString(1)));
                    }
                }

                using (var command = new NpgsqlCommand(sqlPositions, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        ComboBoxItemsPositions.Add(new ComboBoxTypePositionUser(reader.GetDouble(0),reader.GetString(1)));
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

    public async Task LoadUserDataAsync()
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
                    ut.type_user,
                    d.id as department_id,
                    d.name as department_name,
                    p.id as position_id,
                    p.name as position_name
                FROM public.""user"" u
                LEFT JOIN public.user_to_user_type utu ON u.id = utu.user_id
                LEFT JOIN public.user_type ut ON utu.user_type_id = ut.id

                LEFT JOIN public.user_to_departments utd ON u.id = utd.user_id
                LEFT JOIN public.departments d ON utd.department_id = d.id

                LEFT JOIN public.user_to_position utp ON u.id = utp.user_id
                LEFT JOIN public.positions p ON utp.position_id = p.id
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
                            FirstUsername = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                            LastUsername = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                            MiddleName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                            Login = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                            BaseSalary = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);

                            var userTypeId = reader.IsDBNull(5) ? 0 : reader.GetDouble(5);
                            var departmentId = reader.IsDBNull(7) ? 0 : reader.GetDouble(7);
                            var positionId = reader.IsDBNull(9) ? 0 : reader.GetDouble(9);

                            SelectedComboBoxItem = ComboBoxItems.FirstOrDefault(x => x.Id == userTypeId);
                            SelectedComboBoxItemDepartment = ComboBoxItemsDepartments.FirstOrDefault(x => x.Id == departmentId);
                            SelectedComboBoxItemPosition = ComboBoxItemsPositions.FirstOrDefault(x => x.Id == positionId);
                        }
                        else
                        {
                            Messageerror = "Пользователь не найден";
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
                Loges.LoggingProcess(level: LogLevel.WARNING, 
                    ex: ex);
                Messageerror = $"Ошибка обновления: {ex.Message}";
                return false;
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

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
