using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class AddUsersUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
    public double Id { get; }

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

    private decimal _baseSalary = 0;
    public decimal BaseSalary 
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

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set
        {
            if (_email != value)
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }
    }

    private string _phone = string.Empty;
    public string Phone
    {
        get => _phone;
        set
        {
            if (_phone != value)
            {
                _phone = value;
                OnPropertyChanged(nameof(Phone));
            }
        }
    }

    public AddUsersUserControlViewModel(double? id = null, string? login = null, string? firstName = null, string? lastName = null, string? middleName = null, decimal? baseSalary = null, string? email = null, string? phone = null)
    {
        Id = id ?? 0;
        Login = login ?? string.Empty;
        FirstUsername = firstName ?? string.Empty;
        LastUsername = lastName ?? string.Empty;
        MiddleName = middleName ?? string.Empty;
        BaseSalary = baseSalary ?? 1.0m;
        Email = email ?? string.Empty;
        Phone = phone ?? string.Empty;
        _ = LoadListTypeToComboBoxAsync();
    }

    public static ICommand CancelCommand
    => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseUserStatusMessage(false)));

    public ICommand ConfirmCommand
        => new RelayCommand(async () => await SaveAsync());

    public bool IsActiveConfirmButton
        => SelectedComboBoxItem != null 
        && SelectedComboBoxItemDepartment != null 
        && SelectedComboBoxItemPosition != null
        && !string.IsNullOrEmpty(Login)
        && !string.IsNullOrEmpty(MiddleName) 
        && !string.IsNullOrEmpty(FirstUsername) 
        && !string.IsNullOrEmpty(LastUsername)
        && BaseSalary > 0;

    private async Task LoadListTypeToComboBoxAsync()
    {
        try
        {
            string sqlUserTypes = "SELECT id, type_user FROM public.user_type";
            string sqlDepartments = "SELECT id, type FROM public.departments";
            string sqlPositions = "SELECT id, type FROM public.positions";

            using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();

            using (var command = new NpgsqlCommand(sqlUserTypes, connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    ComboBoxItems.Add(new ComboBoxTypeRolsUser(
                        reader.GetDouble(0),
                        reader.GetString(1)
                    ));
                }
            }

            using (var command = new NpgsqlCommand(sqlDepartments, connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    ComboBoxItemsDepartments.Add(new ComboBoxTypeDepartmentUser(
                        reader.GetDouble(0),
                        reader.GetString(1)
                    ));
                }
            }

            using (var command = new NpgsqlCommand(sqlPositions, connection))
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    ComboBoxItemsPositions.Add(new ComboBoxTypePositionUser(
                        reader.GetDouble(0),
                        reader.GetString(1)
                    ));
                }
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING, 
                ex: ex);
        }
    }

    private async Task SaveAsync() 
    {
        try
        {
            if (string.IsNullOrEmpty(Login) || string.IsNullOrEmpty(FirstUsername) || string.IsNullOrEmpty(LastUsername) || string.IsNullOrEmpty(MiddleName) 
                || SelectedComboBoxItem == null || SelectedComboBoxItemDepartment == null || SelectedComboBoxItemPosition == null)
            {
                Messageerror = "Не все поля заполнены";
                return;
            }

            try
            {
                string addUserSql = "INSERT INTO public.user (login, first_name, last_name, middle_name, base_salary, email, phone) " +
                        "VALUES (@login, @first_name, @last_name, @middle_name, @base_salary, @email, @phone) RETURNING id";
                string updateUserSql = "UPDATE public.user SET login = @newLogin, middle_name = @newMiddleName, first_name = @newFirstName, last_name =     @newLastName, base_salary = @newBaseSalary WHERE id = @userID";

                string sql = string.Empty;

                if(Id == 0) sql = addUserSql;
                else sql = updateUserSql;

                using var connection = new NpgsqlConnection(Arguments.Connection);
                await connection.OpenAsync();
                using var command = new NpgsqlCommand(sql, connection);
                try
                {
                    if (Id == 0)
                    {
                        command.Parameters.AddWithValue("@login", Login);
                        command.Parameters.AddWithValue("@first_name", FirstUsername);
                        command.Parameters.AddWithValue("@last_name", LastUsername);
                        command.Parameters.AddWithValue("@middle_name", MiddleName);
                        command.Parameters.AddWithValue("@base_salary", BaseSalary);
                        command.Parameters.AddWithValue("@email", Email);
                        command.Parameters.AddWithValue("@phone", Phone);

                        var userId = await command.ExecuteScalarAsync();
                        if (userId == null) 
                        {
                            Loges.LoggingProcess(level: LogLevel.WARNING,
                                "User id null");
                            return;
                        }

                        string sql2 = "INSERT INTO public.user_to_user_type (user_id, user_type_id) VALUES (@user_id, @user_type_id)";
                        using (var commandRel = new NpgsqlCommand(sql2, connection))
                        {
                            commandRel.Parameters.AddWithValue("@user_id", userId);
                            commandRel.Parameters.AddWithValue("@user_type_id", SelectedComboBoxItem?.Id ?? 0);
                            await commandRel.ExecuteNonQueryAsync();
                        }

                        string sql3 = "INSERT INTO public.user_to_departments (user_id, department_id) VALUES (@user_id, @department_id)";
                        using (var commandDepartment = new NpgsqlCommand(sql3, connection))
                        {
                            commandDepartment.Parameters.AddWithValue("@user_id", userId);
                            commandDepartment.Parameters.AddWithValue("@department_id", SelectedComboBoxItemDepartment?.Id ?? 0);
                            await commandDepartment.ExecuteNonQueryAsync();
                        }

                        string sql4 = "INSERT INTO public.user_to_position (user_id, position_id) VALUES (@user_id, @position_id)";
                        using (var commandPosition = new NpgsqlCommand(sql4, connection))
                        {
                            commandPosition.Parameters.AddWithValue("@user_id", userId);
                            commandPosition.Parameters.AddWithValue("@position_id", SelectedComboBoxItemPosition?.Id ?? 0);
                            await commandPosition.ExecuteNonQueryAsync();
                        }

                        WeakReferenceMessenger.Default.Send(new RefreshUserListMessage());
                        WeakReferenceMessenger.Default.Send(new OpenOrCloseUserStatusMessage(false));

                        ClearForm();
                    }
                    else
                    {
                        command.Parameters.AddWithValue("@userID", Id);
                        command.Parameters.AddWithValue("@newLogin", Login);
                        command.Parameters.AddWithValue("@newFirstName", FirstUsername);
                        command.Parameters.AddWithValue("@newLastName", LastUsername);
                        command.Parameters.AddWithValue("@newMiddleName", MiddleName);
                        command.Parameters.AddWithValue("@newBaseSalary", BaseSalary);
                        await command.ExecuteNonQueryAsync();

                        string deleteSql = "DELETE FROM public.user_to_user_type WHERE user_id = @userID";
                        using (var deleteCommand = new NpgsqlCommand(deleteSql, connection))
                        {
                            deleteCommand.Parameters.AddWithValue("@userID", Id);
                            await deleteCommand.ExecuteNonQueryAsync();
                        }

                        string insertSql = "INSERT INTO public.user_to_user_type (user_id, user_type_id) VALUES (@userID, @userTypeID)";
                        using (var insertCommand = new NpgsqlCommand(insertSql, connection))
                        {
                            insertCommand.Parameters.AddWithValue("@userID", Id);
                            insertCommand.Parameters.AddWithValue("@userTypeID", SelectedComboBoxItem?.Id ?? 0);
                            await insertCommand.ExecuteNonQueryAsync();
                        }

                        WeakReferenceMessenger.Default.Send(new RefreshUserListMessage());
                        WeakReferenceMessenger.Default.Send(new OpenOrCloseUserStatusMessage(false));

                        ClearForm();
                    }
                }
                catch (Exception ex)
                {
                    Loges.LoggingProcess(level: LogLevel.ERROR,
                        ex: ex);
                }
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.WARNING,
                    ex: ex);
                Messageerror = "Неизвестная ошибка";
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);
        }
    }

    public void ClearForm()
    {
        Messageerror = string.Empty;
        ComboBoxItems.Clear();
        MiddleName = string.Empty;
        FirstUsername = string.Empty;
        LastUsername = string.Empty;
        BaseSalary = 0;
        Login = string.Empty;
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
