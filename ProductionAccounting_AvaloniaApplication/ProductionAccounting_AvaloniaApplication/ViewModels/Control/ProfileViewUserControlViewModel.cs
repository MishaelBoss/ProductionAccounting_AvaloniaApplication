using Avalonia.Controls;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class ProfileViewUserControlViewModel : ViewModelBase
{
    public StackPanel? TableUserContent { get; set; } = null;
    public StackPanel? TasksUserContent { get; set; } = null;

    private List<CartTimesheetUserControl> timesheetList = [];
    private List<CartTaskUserControl> tasksList = [];

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

    private string _firstName = string.Empty;
    public string FirstName
    {
        get => _firstName;
        set => this.RaiseAndSetIfChanged(ref _firstName, value);
    }

    private string _lastName = string.Empty;
    public string LastName
    {
        get => _lastName;
        set => this.RaiseAndSetIfChanged(ref _lastName, value);
    }

    private string _middleName = string.Empty;
    public string MiddleName
    {
        get => _middleName;
        set => this.RaiseAndSetIfChanged(ref _middleName, value);
    }

    private string _position = string.Empty;
    public string Position
    {
        get => _position;
        set => this.RaiseAndSetIfChanged(ref _position, value);
    }

    private string _employee = string.Empty;
    public string Employee
    {
        get => _employee;
        set => this.RaiseAndSetIfChanged(ref _employee, value);
    }

    private string _department = string.Empty;
    public string Department
    {
        get => _department;
        set => this.RaiseAndSetIfChanged(ref _department, value);
    }

    private string _userType = string.Empty;
    public string UserType
    {
        get => _userType;
        set => this.RaiseAndSetIfChanged(ref _userType, value);
    }

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set => this.RaiseAndSetIfChanged(ref _email, value);
    }

    private string _phone = string.Empty;
    public string Phone
    {
        get => _phone;
        set => this.RaiseAndSetIfChanged(ref _phone, value);
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => this.RaiseAndSetIfChanged(ref _password, value);
    }

    private string _isActive = string.Empty;
    public string IsActive
    {
        get => _isActive;
        set => this.RaiseAndSetIfChanged(ref _isActive, value);
    }

    public async void InitDateUserAsync()
    {
        if (!ManagerCookie.IsUserLoggedIn()) return;

        await LoadDateAsync();
        await LoadListTypeToComboBoxAsync();
        await LoadTimesheet();
        await LoadTask();
    }

    public async Task LoadDateAsync()
    {
        try
        {
            string sql = @"SELECT login, first_name, last_name, middle_name, email, phone, employee_id, is_active, password FROM public.""user"" WHERE id = @id";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", UserID);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            Login = reader.IsDBNull(0) ? "none" : reader.GetString(0);
                            FirstName = reader.IsDBNull(1) ? "none" : reader.GetString(1);
                            LastName = reader.IsDBNull(2) ? "none" : reader.GetString(2);
                            MiddleName = reader.IsDBNull(3) ? "none" : reader.GetString(3);
                            Email = reader.IsDBNull(4) ? "none" : reader.GetString(4);
                            Phone = reader.IsDBNull(5) ? "none" : reader.GetString(5);
                            Employee = reader.IsDBNull(6) ? "none" : reader.GetString(6);
                            IsActive = reader.IsDBNull(7) ? "none" : reader.GetBoolean(7).ToString();
                            Password = reader.IsDBNull(8) ? "none" : reader.GetString(8);
                        }
                    }
                }
            }
        }
        catch (NpgsqlException ex)
        {
            Loges.LoggingProcess(LogLevel.CRITICAL,
                "Connection or request error",
                ex: ex);
        }
    }

    public async Task LoadListTypeToComboBoxAsync()
    {
        try
        {
            string sql = @"
                        SELECT 
                            ut.type_user,
                            d.type as department,
                            p.type as position
                        FROM public.user_to_user_type utt
                        LEFT JOIN public.user_type ut ON utt.user_type_id = ut.id
                        LEFT JOIN public.user_to_departments ud ON ud.user_id = utt.user_id
                        LEFT JOIN public.departments d ON ud.department_id = d.id
                        LEFT JOIN public.user_to_position up ON up.user_id = utt.user_id
                        LEFT JOIN public.positions p ON up.position_id = p.id
                        WHERE utt.user_id = @userID";

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
                            UserType = reader.IsDBNull(0) ? "none" : reader.GetString(0);
                            Department = reader.IsDBNull(1) ? "none" : reader.GetString(1);
                            Position = reader.IsDBNull(2) ? "none" : reader.GetString(2);
                        }
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

    private async Task LoadTimesheet()
    {
        StackPanelHelper.ClearAndRefreshStackPanel<CartTimesheetUserControl>(TableUserContent, timesheetList);

        try
        {
            string sql = @"SELECT status, notes, hours_worked, work_date FROM public.timesheet WHERE user_id = @userID";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@userID", UserID);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var viewModel = new CartTimesheetUserControlViewModel()
                            {
                                Status = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                                Notes = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                HoursWorked = reader.GetDecimal(2),
                                WorkDate = reader.GetDateTime(3)
                            };

                            var cartUser = new CartTimesheetUserControl()
                            {
                                DataContext = viewModel
                            };

                            timesheetList.Add(cartUser);
                        }
                    }
                }
            }

            StackPanelHelper.RefreshStackPanelContent<CartTimesheetUserControl>(TableUserContent, timesheetList);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);
        }
    }

    private async Task LoadTask()
    {
        StackPanelHelper.ClearAndRefreshStackPanel<CartTaskUserControl>(TasksUserContent, tasksList);

        try
        {
            var sql = @"
                SELECT 
                    p.id,
                    p.quantity, 
                    p.amount, 
                    p.notes, 
                    p.created_at, 
                    p.status,
                    pr.name AS product_name, pr.unit,
                    COALESCE(o.name, '—') AS operation_name
                FROM public.production p
                JOIN public.product pr ON pr.id = p.product_id
                LEFT JOIN public.operation o ON o.id = p.operation_id
                WHERE p.user_id = @user_id 
                ORDER BY p.created_at DESC";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@user_id", UserID);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var viewModel = new CartTaskUserControlViewModel()
                            {
                                IsWork = false,
                                ProductionId = reader.GetDouble(0),
                                Quantity = reader.GetDecimal(1),
                                Amount = reader.GetDecimal(2),
                                Notes = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                CreatedAt = reader.GetDateTime(4),
                                Status = reader.GetString(5),
                                ProductName = reader.GetString(6),
                                Unit = reader.GetString(7),
                                OperationName = reader.GetString(8)
                            };

                            var cartUser = new CartTaskUserControl()
                            {
                                DataContext = viewModel
                            };

                            tasksList.Add(cartUser);
                        }
                    }
                }
            }

            StackPanelHelper.RefreshStackPanelContent<CartTaskUserControl>(TasksUserContent, tasksList);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);
        }
    }
}
