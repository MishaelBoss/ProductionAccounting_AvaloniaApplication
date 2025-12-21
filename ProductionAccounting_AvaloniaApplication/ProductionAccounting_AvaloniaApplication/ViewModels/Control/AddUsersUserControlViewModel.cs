using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class AddUsersUserControlViewModel : ViewModelBase
{
    private double Id { get; }

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
            this.RaiseAndSetIfChanged(ref _selectedComboBoxItem, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
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
            this.RaiseAndSetIfChanged(ref _selectedComboBoxItemDepartment, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
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
            this.RaiseAndSetIfChanged(ref _selectedComboBoxItemPosition, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
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

    private string _firstUsername = string.Empty;
    [UsedImplicitly]
    public string FirstUsername
    {
        get => _firstUsername;
        set
        {
            this.RaiseAndSetIfChanged(ref _firstUsername, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    private string _lastUsername = string.Empty;
    [UsedImplicitly]
    public string LastUsername
    {
        get => _lastUsername;
        set
        {
            this.RaiseAndSetIfChanged(ref _lastUsername, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    private string _middleName = string.Empty;
    [UsedImplicitly]
    public string MiddleName
    {
        get => _middleName;
        set
        {
            this.RaiseAndSetIfChanged(ref _middleName, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    private decimal _baseSalary;
    [UsedImplicitly]
    public decimal BaseSalary 
    {
        get => _baseSalary;
        set 
        {
            this.RaiseAndSetIfChanged(ref _baseSalary, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    private string? _email = string.Empty;
    [UsedImplicitly]
    public string? Email
    {
        get => _email;
        set => this.RaiseAndSetIfChanged(ref _email, value);
    }

    private string? _phone = string.Empty;
    [UsedImplicitly]
    public string? Phone
    {
        get => _phone;
        set => this.RaiseAndSetIfChanged(ref _phone, value);
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
        => new RelayCommand(async void () =>
        {
            try
            {
                await SaveAsync();
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.Critical, 
                    ex: ex, 
                    message: "Error add or update");
            }
        });

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
            const string sqlUserTypes = "SELECT id, type_user FROM public.user_type";
            const string sqlDepartments = "SELECT id, type FROM public.departments";
            const string sqlPositions = "SELECT id, type FROM public.positions";

            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();

            await using (var command = new NpgsqlCommand(sqlUserTypes, connection))
            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    ComboBoxItems.Add(new ComboBoxTypeRolsUser(
                        reader.GetDouble(0),
                        reader.GetString(1)
                    ));
                }
            }

            await using (var command = new NpgsqlCommand(sqlDepartments, connection))
            await using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    ComboBoxItemsDepartments.Add(new ComboBoxTypeDepartmentUser(
                        reader.GetDouble(0),
                        reader.GetString(1)
                    ));
                }
            }

            await using (var command = new NpgsqlCommand(sqlPositions, connection))
            await using (var reader = await command.ExecuteReaderAsync())
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
            Loges.LoggingProcess(level: LogLevel.Warning, 
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
            
            const string addUserSql = "INSERT INTO public.user (login, first_name, last_name, middle_name, base_salary, email, phone) " +
                    "VALUES (@login, @first_name, @last_name, @middle_name, @base_salary, @email, @phone) RETURNING id";
            const string updateUserSql = "UPDATE public.user SET login = @newLogin, middle_name = @newMiddleName, first_name = @newFirstName, last_name=     @newLastName, base_salary = @newBaseSalary WHERE id = @userID";
            
            var sql = Id == 0 ? updateUserSql : addUserSql;
            
            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand(sql, connection);
            
            command.Parameters.AddWithValue("@login", Login.Trim());
            command.Parameters.AddWithValue("@first_name", FirstUsername.Trim());
            command.Parameters.AddWithValue("@last_name", LastUsername.Trim());
            command.Parameters.AddWithValue("@middle_name", MiddleName.Trim());
            command.Parameters.AddWithValue("@base_salary", BaseSalary);
            command.Parameters.AddWithValue("@email", Email?.Trim() ?? string.Empty);
            command.Parameters.AddWithValue("@phone", Phone?.Trim() ?? string.Empty);
            
            if (Id == 0)
            {
                var userId = await command.ExecuteScalarAsync();
                if (userId is null) 
                {
                    Loges.LoggingProcess(level: LogLevel.Warning,
                        "User id null");
                    return;
                }
                
                const string sql2 = "INSERT INTO public.user_to_user_type (user_id, user_type_id) VALUES (@user_id, @user_type_id)";
                await using (var commandRel = new NpgsqlCommand(sql2, connection))
                {
                    commandRel.Parameters.AddWithValue("@user_id", userId);
                    commandRel.Parameters.AddWithValue("@user_type_id", SelectedComboBoxItem?.Id ?? 0);
                    await commandRel.ExecuteNonQueryAsync();
                }
                
                const string sql3 = "INSERT INTO public.user_to_departments (user_id, department_id) VALUES (@user_id, @departmen";
                await using (var commandDepartment = new NpgsqlCommand(sql3, connection))
                {
                    commandDepartment.Parameters.AddWithValue("@user_id", userId);
                    commandDepartment.Parameters.AddWithValue("@department_id", SelectedComboBoxItemDepartment?.Id ?? 0);
                    await commandDepartment.ExecuteNonQueryAsync();
                }
                
                const string sql4 = "INSERT INTO public.user_to_position (user_id, position_id) VALUES (@user_id, @position_id)";
                await using (var commandPosition = new NpgsqlCommand(sql4, connection))
                {
                    commandPosition.Parameters.AddWithValue("@user_id", userId);
                    commandPosition.Parameters.AddWithValue("@position_id", SelectedComboBoxItemPosition?.Id ?? 0);
                    await commandPosition.ExecuteNonQueryAsync();
                }
            }
            else
            {
                await command.ExecuteNonQueryAsync();
                const string deleteSql = "DELETE FROM public.user_to_user_type WHERE user_id = @userID";
                await using (var deleteCommand = new NpgsqlCommand(deleteSql, connection))
                {
                    deleteCommand.Parameters.AddWithValue("@userID", Id);
                    await deleteCommand.ExecuteNonQueryAsync();
                }
                const string insertSql = "INSERT INTO public.user_to_user_type (user_id, user_type_id) VALUES (@userID, @userTypeID)";
                await using (var insertCommand = new NpgsqlCommand(insertSql, connection))
                {
                    insertCommand.Parameters.AddWithValue("@userID", Id);
                    insertCommand.Parameters.AddWithValue("@userTypeID", SelectedComboBoxItem?.Id ?? 0);
                    await insertCommand.ExecuteNonQueryAsync();
                }
            }    
            WeakReferenceMessenger.Default.Send(new OpenOrCloseUserStatusMessage(false));
            WeakReferenceMessenger.Default.Send(new RefreshUserListMessage());
            ClearForm();
        }
        catch (PostgresException ex)
        {
            Loges.LoggingProcess(
                level: LogLevel.Warning,
                ex: ex,
                message: $"Error DB (SQLState: {ex.SqlState}): {ex.MessageText}");

            Loges.LoggingProcess(
                level: LogLevel.Warning,
                ex: ex,
                message: $"Error DB (Detail: {ex.Detail})");

            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
    }

    private void ClearForm()
    {
        Messageerror = string.Empty;
        ComboBoxItems.Clear();
        MiddleName = string.Empty;
        FirstUsername = string.Empty;
        LastUsername = string.Empty;
        BaseSalary = 0;
        Login = string.Empty;
    }
}
