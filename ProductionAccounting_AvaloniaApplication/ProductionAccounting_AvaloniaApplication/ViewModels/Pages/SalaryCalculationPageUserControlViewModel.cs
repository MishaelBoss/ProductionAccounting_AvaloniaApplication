using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using ReactiveUI;
using static ProductionAccounting_AvaloniaApplication.ViewModels.Control.NotFoundUserControlViewModel;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class SalaryCalculationPageUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
    public StackPanel? SalaryContent { get; set; } = null;
    
    private List<SalaryRecordUserControl> salaryList = [];
    
    private DateTimeOffset _periodStart = new(DateTime.Now.AddMonths(-1));
    public DateTimeOffset PeriodStart
    {
        get => _periodStart;
        set => this.RaiseAndSetIfChanged(ref _periodStart, value);
    }
    
    private DateTimeOffset _periodEnd = new(DateTime.Now);
    public DateTimeOffset PeriodEnd
    {
        get => _periodEnd;
        set => this.RaiseAndSetIfChanged(ref _periodEnd, value);
    }
    
    private double? _selectedUserId;
    public double? SelectedUserId
    {
        get => _selectedUserId;
        set => this.RaiseAndSetIfChanged(ref _selectedUserId, value);
    }
    
    private List<UserComboBoxItem> _users = [];
    public List<UserComboBoxItem> Users
    {
        get => _users;
        set => this.RaiseAndSetIfChanged(ref _users, value);
    }
    
    public ICommand CalculateSalaryCommand
        => new RelayCommand(async () => await CalculateSalaryAsync());
    
    public ICommand GenerateReportCommand
        => new RelayCommand(async () => await GenerateReportAsync());
    
    public ICommand LoadUsersCommand
        => new RelayCommand(async () => await LoadUsersAsync());
    
    public SalaryCalculationPageUserControlViewModel()
    {
        _ = LoadUsersAsync();
    }
    
    private async Task LoadUsersAsync()
    {
        try
        {
            string sql = @"
                    SELECT 
                        id, 
                        first_name, 
                        last_name, 
                        middle_name, 
                        login 
                    FROM public.user 
                    WHERE is_active = true 
                    ORDER BY last_name, first_name";
            
            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var users = new List<UserComboBoxItem>
                        {
                            new UserComboBoxItem(0, "Все сотрудники", "", "", "")
                        };
                        
                        while (await reader.ReadAsync())
                        {
                            users.Add(new UserComboBoxItem(
                                reader.GetDouble(0),
                                reader.GetString(1),
                                reader.GetString(2),
                                reader.GetString(3),
                                reader.GetString(4)
                            ));
                        }
                        
                        Users = users;
                        SelectedUserId = 0;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.ERROR, "Ошибка загрузки пользователей", ex: ex);
        }
    }
    
    private async Task CalculateSalaryAsync()
    {
        StackPanelHelper.ClearAndRefreshStackPanel<SalaryRecordUserControl>(SalaryContent, salaryList);
        
        try
        {
            var parameters = new List<NpgsqlParameter>();
            var sql = @"
                SELECT 
                    p.user_id,
                    u.first_name || ' ' || u.last_name AS employee_name,
                    o.name AS operation_name,
                    p.production_date,
                    p.quantity,
                    p.tonnage,
                    p.notes,
                    wr.rate,
                    wr.use_tonnage,
                    wr.coefficient,
                    CASE 
                        WHEN wr.use_tonnage THEN 
                            wr.rate * COALESCE(p.tonnage, 0) / 1000 * p.quantity * wr.coefficient
                        ELSE 
                            wr.rate * p.quantity
                    END AS calculated_amount,
                    p.calculation_formula
                FROM public.production p
                JOIN public.user u ON u.id = p.user_id
                JOIN public.operation o ON o.id = p.operation_id
                JOIN public.work_rate wr ON wr.work_type = o.name
                WHERE p.production_date BETWEEN @startDate AND @endDate
                    AND p.status = 'issued'
            ";
            
            if (SelectedUserId.HasValue && SelectedUserId.Value > 0)
            {
                sql += " AND p.user_id = @userId";
                parameters.Add(new NpgsqlParameter("@userId", SelectedUserId.Value));
            }
            
            sql += " ORDER BY p.user_id, p.production_date";
            
            parameters.Add(new NpgsqlParameter("@startDate", PeriodStart.DateTime));
            parameters.Add(new NpgsqlParameter("@endDate", PeriodEnd.DateTime));
            
            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(param);
                    }
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        decimal totalAmount = 0;
                        var userSummaries = new Dictionary<double, UserSalarySummary>();
                        
                        while (await reader.ReadAsync())
                        {
                            var userId = reader.GetDouble(0);
                            var amount = reader.GetDecimal(11);
                            
                            totalAmount += amount;
                            
                            if (!userSummaries.ContainsKey(userId))
                            {
                                userSummaries[userId] = new UserSalarySummary
                                {
                                    UserId = userId,
                                    EmployeeName = reader.GetString(1),
                                    TotalAmount = 0
                                };
                            }
                            userSummaries[userId].TotalAmount += amount;
                            
                            var viewModel = new SalaryRecordUserControlViewModel
                            {
                                EmployeeName = reader.GetString(1),
                                OperationName = reader.GetString(2),
                                ProductionDate = reader.GetDateTime(3),
                                Quantity = reader.GetDecimal(4),
                                Tonnage = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                                Amount = amount,
                                CalculationFormula = reader.IsDBNull(12) ? "" : reader.GetString(12)
                            };
                            
                            var control = new SalaryRecordUserControl
                            {
                                DataContext = viewModel
                            };
                            
                            salaryList.Add(control);
                        }
                        
                        foreach (var summary in userSummaries.Values)
                        {
                            var summaryViewModel = new SalaryRecordUserControlViewModel
                            {
                                EmployeeName = $"{summary.EmployeeName} (Итого)",
                                Amount = summary.TotalAmount,
                                IsSummary = true
                            };
                            
                            var summaryControl = new SalaryRecordUserControl
                            {
                                DataContext = summaryViewModel
                            };
                            
                            salaryList.Add(summaryControl);
                        }
                        
                        var totalViewModel = new SalaryRecordUserControlViewModel
                        {
                            EmployeeName = "ОБЩИЙ ИТОГ",
                            Amount = totalAmount,
                            IsTotal = true
                        };
                        
                        var totalControl = new SalaryRecordUserControl
                        {
                            DataContext = totalViewModel
                        };
                        
                        salaryList.Add(totalControl);
                    }
                }
            }
            
            StackPanelHelper.RefreshStackPanelContent<SalaryRecordUserControl>(SalaryContent, salaryList);
            
            if (salaryList.Count == 0)
                ItemNotFoundException.Show(SalaryContent, ErrorLevel.NotFound);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.ERROR, "Ошибка расчета зарплаты", ex: ex);
            StackPanelHelper.ClearAndRefreshStackPanel<SalaryRecordUserControl>(SalaryContent, salaryList);
            ItemNotFoundException.Show(SalaryContent, ErrorLevel.NoConnectToDB);
        }
    }
    
    private async Task GenerateReportAsync()
    {
        try
        {
            var sql = @"
                SELECT 
                    u.id,
                    u.first_name,
                    u.last_name,
                    u.middle_name,
                    u.base_salary,
                    SUM(CASE 
                        WHEN wr.use_tonnage THEN 
                            wr.rate * COALESCE(p.tonnage, 0) / 1000 * p.quantity * wr.coefficient
                        ELSE 
                            wr.rate * p.quantity
                    END) AS production_amount,
                    SUM(CASE 
                        WHEN wr.use_tonnage THEN 
                            wr.rate * COALESCE(p.tonnage, 0) / 1000 * p.quantity * wr.coefficient
                        ELSE 
                            wr.rate * p.quantity
                    END) + u.base_salary AS total_amount
                FROM public.user u
                LEFT JOIN public.production p ON p.user_id = u.id 
                    AND p.production_date BETWEEN @startDate AND @endDate
                    AND p.status = 'issued'
                LEFT JOIN public.operation o ON o.id = p.operation_id
                LEFT JOIN public.work_rate wr ON wr.work_type = o.name
                WHERE u.is_active = true
            ";
            
            if (SelectedUserId.HasValue && SelectedUserId.Value > 0)
            {
                sql += " AND u.id = @userId";
            }
            
            sql += @" 
                GROUP BY u.id, u.first_name, u.last_name, u.middle_name, u.base_salary
                ORDER BY u.last_name, u.first_name
            ";
            
            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@startDate", PeriodStart.DateTime);
                    command.Parameters.AddWithValue("@endDate", PeriodEnd.DateTime);
                    
                    if (SelectedUserId.HasValue && SelectedUserId.Value > 0)
                    {
                        command.Parameters.AddWithValue("@userId", SelectedUserId.Value);
                    }
                    
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        Loges.LoggingProcess(LogLevel.INFO, "Отчет сгенерирован");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.ERROR, "Ошибка генерации отчета", ex: ex);
        }
    }
    
    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}

public class UserSalarySummary
{
    public double UserId { get; set; }
    public string EmployeeName { get; set; } = "";
    public decimal TotalAmount { get; set; }
}

public class UserComboBoxItem
{
    public double Id { get; set; }
    public string DisplayName { get; set; }

    public UserComboBoxItem(double id, string firstName, string lastName, string middleName, string login)
    {
        Id = id;
        DisplayName = id == 0 ? "Все сотрудники" : $"{lastName} {firstName} {middleName} ({login})";
    }
}
