using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class ProfilePageUserControlViewModel : ViewModelBase
{
    private string _employeeName = string.Empty;
    [UsedImplicitly]
    public string EmployeeName
    {
        get => _employeeName;
        set => this.RaiseAndSetIfChanged(ref _employeeName, value);
    }

    private string _login = string.Empty;
    [UsedImplicitly]
    public string Login
    {
        get => _login;
        set => this.RaiseAndSetIfChanged(ref _login, value);
    }

    private string _firstName = string.Empty;
    [UsedImplicitly]
    public string FirstName
    {
        get => _firstName;
        set => this.RaiseAndSetIfChanged(ref _firstName, value);
    }

    private string _lastName = string.Empty;
    [UsedImplicitly]
    public string LastName
    {
        get => _lastName;
        set => this.RaiseAndSetIfChanged(ref _lastName, value);
    }

    private string _middleName = string.Empty;
    [UsedImplicitly]
    public string MiddleName
    {
        get => _middleName;
        set => this.RaiseAndSetIfChanged(ref _middleName, value);
    }

    private string _employee = string.Empty;
    [UsedImplicitly]
    public string Employee
    {
        get => _employee;
        set => this.RaiseAndSetIfChanged(ref _employee, value);
    }

    private string _email = string.Empty;
    [UsedImplicitly]
    public string Email
    {
        get => _email;
        set => this.RaiseAndSetIfChanged(ref _email, value);
    }

    private string _phone = string.Empty;
    [UsedImplicitly]
    public string Phone
    {
        get => _phone;
        set => this.RaiseAndSetIfChanged(ref _phone, value);
    }

    private ObservableCollection<CartTimesheetUserControlViewModel> _tables = [];
    public ObservableCollection<CartTimesheetUserControlViewModel> Tables
    {
        get => _tables;
        set => this.RaiseAndSetIfChanged(ref _tables, value);
    }

    private ObservableCollection<ProductionHistoryViewModel> _productionHistory = [];
    public ObservableCollection<ProductionHistoryViewModel> ProductionHistory
    {
        get => _productionHistory;
        set => this.RaiseAndSetIfChanged(ref _productionHistory, value);
    }

    public static ICommand LogoutCommand
        => new RelayCommand(() =>
        {
            ManagerCookie.DeleteCookie();
            StrongReferenceMessenger.Default.Send(new UserAuthenticationChangedMessage());
        });

    public ProfilePageUserControlViewModel(double userId)
    {
        InitDateUserAsync(userId);
    }

    private async void InitDateUserAsync(double userId)
    {
        try
        {
            if (!ManagerCookie.IsUserLoggedIn()) return;

            await LoadDateAsync(userId);
            await LoadTimesheet(userId);
            await LoadProductionHistory(userId);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Critical,
                ex: ex,
                message: "Error init db user profile");
        }
    }

    private double? _getIdUser;
    private double? GetIdUser
    {
        get => _getIdUser;
        set 
        {
            this.RaiseAndSetIfChanged(ref _getIdUser, value);
            this.RaisePropertyChanged(nameof(IsOwnProfile));
        }
    }

    public bool IsOwnProfile
    {
        get
        {
            if (!ManagerCookie.IsUserLoggedIn()) return false;

            var currentId = (long)(GetIdUser ?? 0);
            var cookieId = (long)(ManagerCookie.GetIdUser ?? 0);

            return currentId != 0 && currentId == cookieId;
        }
    }

    private async Task LoadDateAsync(double userId)
    {
        if (!ManagerCookie.IsUserLoggedIn()) return;

        try
        {
            const string sql = @"SELECT login, first_name, last_name, middle_name, email, phone, employee_id, id FROM public.""user"" WHERE id = @id";

            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", userId);

            await using var reader = await command.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                Login = reader.IsDBNull(0) ? "none" : reader.GetString(0);
                FirstName = reader.IsDBNull(1) ? "none" : reader.GetString(1);
                LastName = reader.IsDBNull(2) ? "none" : reader.GetString(2);
                MiddleName = reader.IsDBNull(3) ? "none" : reader.GetString(3);
                Email = reader.IsDBNull(4) ? "none" : reader.GetString(4);
                Phone = reader.IsDBNull(5) ? "none" : reader.GetString(5);
                Employee = reader.IsDBNull(6) ? "none" : reader.GetString(6);

                GetIdUser = reader.GetDouble(7);

                EmployeeName = $"{FirstName} {LastName} {MiddleName}";
            }
        }
        catch (NpgsqlException ex)
        {
            Loges.LoggingProcess(LogLevel.Critical,
                "Connection or request error",
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.Critical,
                "Connection or request error",
                ex: ex);
        }
    }

    private async Task LoadTimesheet(double userId)
    {
        try
        {
            const string sql = @"SELECT status, notes, hours_worked, work_date FROM public.timesheet WHERE user_id = @userID";

            await using (var connection = new NpgsqlConnection(Arguments.Connection))
            {
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@userID", userId);

                await using var reader = await command.ExecuteReaderAsync();
                Tables.Clear();

                while (await reader.ReadAsync())
                {
                    var viewModel = new CartTimesheetUserControlViewModel()
                    {
                        Status = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                        Notes = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        HoursWorked = reader.GetDecimal(2),
                        WorkDate = reader.GetDateTime(3)
                    };

                    Tables.Add(viewModel);
                }
            }

            this.RaisePropertyChanged(nameof(Tables));
        }
        catch (NpgsqlException ex)
        {
            Loges.LoggingProcess(LogLevel.Critical,
                "Connection or request error",
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
    }

    private async Task LoadProductionHistory(double userId)
    {
        try
        {
            const string sql = @"
                        SELECT 
                            p.production_date,
                            p.shift,
                            COALESCE(pr.name, 'Продукт не найден'),
                            COALESCE(o.name, 'Операция не указана'),
                            COALESCE(o.unit, 'шт'),
                            p.quantity,
                            p.amount,
                            p.tonnage,
                            p.status,
                            p.notes,
                            p.created_at
                        FROM public.production p
                        LEFT JOIN public.product pr ON pr.id = p.product_id
                        LEFT JOIN public.operation o ON o.id = p.operation_id
                        WHERE p.user_id = @user_id
                        ORDER BY p.production_date DESC, p.created_at DESC";

            await using (var connection = new NpgsqlConnection(Arguments.Connection))
            {
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@user_id", userId);

                await using var reader = await command.ExecuteReaderAsync();
                ProductionHistory.Clear();

                while (await reader.ReadAsync())
                {
                    var productionItem = new ProductionHistoryViewModel()
                    {
                        ProductionDate = reader.GetDateTime(0),
                        Shift = reader.IsDBNull(1) ? null : reader.GetInt32(1),
                        ProductName = reader.GetString(2),
                        OperationName = reader.GetString(3),
                        OperationUnit = reader.GetString(4),
                        Quantity = reader.GetDecimal(5),
                        Amount = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6),
                        Tonnage = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7),
                        Status = reader.GetString(8),
                        Notes = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                        CreatedAt = reader.GetDateTime(10)
                    };

                    ProductionHistory.Add(productionItem);
                }
            }

            this.RaisePropertyChanged(nameof(ProductionHistory));
        }
        catch (NpgsqlException ex)
        {
            Loges.LoggingProcess(LogLevel.Critical,
                "Connection or request error",
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
    }
}
