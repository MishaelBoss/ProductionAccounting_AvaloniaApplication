using Avalonia;
using Avalonia.Controls;
using Avalonia.Layout;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class SalaryCalculationPageUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
    public StackPanel? SalaryContent { get; set; } = null;

    private readonly List<SalaryRecordUserControl> salaryList = [];

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

    private UserComboBoxItem? _selectedUser;
    public UserComboBoxItem? SelectedUser
    {
        get => _selectedUser;
        set => this.RaiseAndSetIfChanged(ref _selectedUser, value);
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

            using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();
            using var command = new NpgsqlCommand(sql, connection);
            using var reader = await command.ExecuteReaderAsync();
            var users = new List<UserComboBoxItem>{ new(0, "Все сотрудники", "", "", "") };

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
            SelectedUser = users[0];
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.ERROR, "Ошибка загрузки пользователей", ex: ex);
        }
    }

    private async Task CalculateSalaryAsync()
    {
        Console.WriteLine($"=== НАЧАЛО РАСЧЕТА ЗАРПЛАТЫ ===");
        Console.WriteLine($"Период: {PeriodStart.DateTime:dd.MM.yyyy} - {PeriodEnd.DateTime:dd.MM.yyyy}");
        Console.WriteLine($"Сотрудник: {SelectedUser?.DisplayName ?? "Все"}");

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
                    o.unit,
                    COALESCE(wr.rate, o.price) AS rate,
                    -- Определяем использование тоннажа
                    COALESCE(wr.use_tonnage, 
                        LOWER(o.unit) IN ('кг', 'kg', 'тонна', 'ton', 'т', 'т.', 'тонны', 'килограмм', 'килограммы')) AS use_tonnage, -- [8]
                    COALESCE(wr.coefficient, 1.0) AS coefficient, -- [9]
                    -- Формула расчета
                    CASE 
                        WHEN COALESCE(wr.use_tonnage, 
                                LOWER(o.unit) IN ('кг', 'kg', 'тонна', 'ton', 'т', 'т.', 'тонны', 'килограмм', 'килограммы')) 
                            AND p.tonnage > 0 
                        THEN 
                            COALESCE(wr.rate, o.price) * p.tonnage / 1000 * p.quantity * COALESCE(wr.coefficient, 1.0)
                        ELSE 
                            COALESCE(wr.rate, o.price) * p.quantity
                    END AS calculated_amount,           -- [10]
                    -- Формула для отображения
                    CASE 
                        WHEN COALESCE(wr.use_tonnage, 
                                LOWER(o.unit) IN ('кг', 'kg', 'тонна', 'ton', 'т', 'т.', 'тонны', 'килограмм', 'килограммы')) 
                            AND p.tonnage > 0 
                        THEN 
                            COALESCE(wr.rate, o.price)::text || ' × ' || p.tonnage::text || ' / 1000 × ' || 
                            p.quantity::text || ' × ' || COALESCE(wr.coefficient, 1.0)::text || ' = ' ||
                            ROUND(
                                COALESCE(wr.rate, o.price) * p.tonnage / 1000 * p.quantity * COALESCE(wr.coefficient, 1.0), 
                                2
                            )::text
                        ELSE 
                            COALESCE(wr.rate, o.price)::text || ' × ' || p.quantity::text || ' = ' || 
                            ROUND(COALESCE(wr.rate, o.price) * p.quantity, 2)::text
                    END AS calculation_formula          -- [11]
                FROM public.production p
                JOIN public.user u ON u.id = p.user_id
                JOIN public.operation o ON o.id = p.operation_id
                LEFT JOIN public.work_rate wr ON LOWER(TRIM(wr.work_type)) = LOWER(TRIM(o.name))
                WHERE p.production_date BETWEEN @startDate AND @endDate
                    AND p.status IN ('issued', 'completed')
                    AND u.is_active = true";

            if (SelectedUser != null && SelectedUser.Id > 0)
            {
                sql += " AND p.user_id = @userId";
                parameters.Add(new NpgsqlParameter("@userId", SelectedUser.Id));
            }

            sql += " ORDER BY p.user_id, p.production_date DESC, o.name";

            parameters.Add(new NpgsqlParameter("@startDate", PeriodStart.DateTime.Date));
            parameters.Add(new NpgsqlParameter("@endDate", PeriodEnd.DateTime.Date.AddDays(1).AddSeconds(-1)));

            Console.WriteLine($"SQL запрос подготовлен");
            Console.WriteLine($"Параметры:");
            Console.WriteLine($"  startDate: {PeriodStart.DateTime:yyyy-MM-dd}");
            Console.WriteLine($"  endDate: {PeriodEnd.DateTime:yyyy-MM-dd}");

            using var connection = new NpgsqlConnection(Arguments.Connection);
            Console.WriteLine("Подключение к БД...");

            connection.ConnectionString = Arguments.Connection + ";Command Timeout=60";

            await connection.OpenAsync();
            Console.WriteLine("Подключение к БД установлено");

            using (var testCmd = new NpgsqlCommand("SELECT 1", connection))
            {
                var testResult = await testCmd.ExecuteScalarAsync();
                Console.WriteLine($"Тест соединения: {(testResult?.ToString() == "1" ? "OK" : "FAILED")}");
            }
            try
            {
                Console.WriteLine("=== ПРОВЕРКА ДАННЫХ ===");
                var checkSql = @"
                    SELECT COUNT(*) 
                    FROM public.production 
                    WHERE production_date BETWEEN @startDate AND @endDate 
                      AND status IN ('issued', 'completed')";

                if (SelectedUser != null && SelectedUser.Id > 0)
                {
                    checkSql += " AND user_id = @userId";
                }

                using var checkCmd = new NpgsqlCommand(checkSql, connection);
                checkCmd.Parameters.AddWithValue("@startDate", PeriodStart.DateTime.Date);
                checkCmd.Parameters.AddWithValue("@endDate", PeriodEnd.DateTime.Date.AddDays(1).AddSeconds(-1));

                if (SelectedUser != null && SelectedUser.Id > 0)
                {
                    checkCmd.Parameters.AddWithValue("@userId", SelectedUser.Id);
                }

                var count = Convert.ToInt64(await checkCmd.ExecuteScalarAsync());
                Console.WriteLine($"Найдено записей: {count}");

                if (count == 0)
                {
                    ShowDetailedNoDataMessage();
                    return;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при проверке данных: {ex.Message}");
            }

            decimal totalAmount = 0;
            var userSummaries = new Dictionary<double, UserSalarySummary>();
            int recordCount = 0;

            try
            {
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(param);
                    }

                    command.CommandTimeout = 30;

                    Console.WriteLine("Выполнение основного запроса...");

                    using var reader = await command.ExecuteReaderAsync();
                    Console.WriteLine("=== ЧТЕНИЕ ДАННЫХ ===");

                    while (await reader.ReadAsync())
                    {
                        recordCount++;

                        try
                        {
                            var userId = reader.GetDouble(0);
                            var employeeName = reader.GetString(1);
                            var operationName = reader.GetString(2);
                            var productionDate = reader.GetDateTime(3);
                            var quantity = reader.GetDecimal(4);
                            var tonnage = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5);
                            var unit = reader.IsDBNull(6) ? "шт" : reader.GetString(6);
                            var rate = reader.GetDecimal(7);
                            var useTonnage = reader.GetBoolean(8);
                            var coefficient = reader.GetDecimal(9);
                            var amount = reader.GetDecimal(10);
                            var calculationFormula = reader.GetString(11);

                            Console.WriteLine($"Запись #{recordCount}:");
                            Console.WriteLine($"  Сотрудник: {employeeName}");
                            Console.WriteLine($"  Операция: {operationName}");
                            Console.WriteLine($"  Дата: {productionDate:dd.MM.yyyy}");
                            Console.WriteLine($"  Кол-во: {quantity}");
                            Console.WriteLine($"  Тоннаж: {tonnage}");
                            Console.WriteLine($"  Единица: {unit}");
                            Console.WriteLine($"  Ставка: {rate}");
                            Console.WriteLine($"  Исп.тоннаж: {useTonnage}");
                            Console.WriteLine($"  Коэффициент: {coefficient}");
                            Console.WriteLine($"  Сумма: {amount:N2} руб");

                            totalAmount += amount;

                            if (!userSummaries.TryGetValue(userId, out UserSalarySummary? value))
                            {
                                value = new UserSalarySummary
                                {
                                    UserId = userId,
                                    EmployeeName = employeeName,
                                    TotalAmount = 0
                                };
                                userSummaries[userId] = value;
                            }

                            value.TotalAmount += amount;

                            var viewModel = new SalaryRecordUserControlViewModel
                            {
                                EmployeeName = employeeName,
                                OperationName = operationName,
                                ProductionDate = productionDate,
                                Quantity = quantity,
                                Tonnage = tonnage,
                                Amount = amount,
                                CalculationFormula = calculationFormula,
                                Unit = unit,
                                Rate = rate,
                                UseTonnage = useTonnage && tonnage > 0,
                                Coefficient = coefficient,
                                IsSummary = false,
                                IsTotal = false
                            };

                            var control = new SalaryRecordUserControl
                            {
                                DataContext = viewModel
                            };

                            salaryList.Add(control);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Ошибка обработки записи #{recordCount}: {ex.Message}");
                            continue;
                        }
                    }
                }

                Console.WriteLine($"=== ИТОГО ===");
                Console.WriteLine($"Всего найдено записей: {recordCount}");
                Console.WriteLine($"Общая сумма: {totalAmount:N2} руб");
                Console.WriteLine($"Количество сотрудников: {userSummaries.Count}");

                foreach (var summary in userSummaries.Values)
                {
                    Console.WriteLine($"  {summary.EmployeeName}: {summary.TotalAmount:N2} руб");

                    var summaryViewModel = new SalaryRecordUserControlViewModel
                    {
                        EmployeeName = $"{summary.EmployeeName} (Итого)",
                        Amount = summary.TotalAmount,
                        IsSummary = true,
                        IsTotal = false
                    };

                    var summaryControl = new SalaryRecordUserControl
                    {
                        DataContext = summaryViewModel
                    };

                    salaryList.Add(summaryControl);
                }

                if (recordCount > 0)
                {
                    var totalViewModel = new SalaryRecordUserControlViewModel
                    {
                        EmployeeName = "ОБЩИЙ ИТОГ",
                        Amount = totalAmount,
                        IsSummary = false,
                        IsTotal = true
                    };

                    var totalControl = new SalaryRecordUserControl
                    {
                        DataContext = totalViewModel
                    };

                    salaryList.Add(totalControl);
                }

                StackPanelHelper.RefreshStackPanelContent<SalaryRecordUserControl>(SalaryContent, salaryList);

                if (salaryList.Count == 0)
                {
                    Console.WriteLine("=== НЕТ ДАННЫХ ===");
                    ShowDetailedNoDataMessage();
                }
                else
                {
                    Console.WriteLine($"=== ДАННЫЕ ОТОБРАЖЕНЫ ===");
                    Console.WriteLine($"Отображено {salaryList.Count} записей");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка выполнения запроса: {ex.Message}");
                Console.WriteLine($"Тип ошибки: {ex.GetType().Name}");
                throw;
            }
        }
        catch (NpgsqlException npgsqlEx)
        {
            Console.WriteLine($"=== ОШИБКА POSTGRESQL ===");
            Console.WriteLine($"Сообщение: {npgsqlEx.Message}");
            Console.WriteLine($"Код ошибки: {npgsqlEx.ErrorCode}");
            Console.WriteLine($"StackTrace: {npgsqlEx.StackTrace}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"=== ОБЩАЯ ОШИБКА ===");
            Console.WriteLine($"Сообщение: {ex.Message}");
            Console.WriteLine($"Тип ошибки: {ex.GetType().Name}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");

            if (ex.InnerException != null)
            {
                Console.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
            }

            Loges.LoggingProcess(LogLevel.ERROR, "Ошибка расчета зарплаты", ex: ex);
            StackPanelHelper.ClearAndRefreshStackPanel<SalaryRecordUserControl>(SalaryContent, salaryList);
        }

        Console.WriteLine($"=== КОНЕЦ РАСЧЕТА ===");
    }

    private void ShowDetailedNoDataMessage()
    {
        var stackPanel = new StackPanel
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(0, 40, 0, 0)
        };

        stackPanel.Children.Add(new TextBlock
        {
            Text = "📊",
            FontSize = 48,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20)
        });

        stackPanel.Children.Add(new TextBlock
        {
            Text = "Данные не найдены",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        });

        var description = new TextBlock
        {
            Text = $"За период {PeriodStart.DateTime:dd.MM.yyyy} - {PeriodEnd.DateTime:dd.MM.yyyy}\n" +
                   $"Сотрудник: {SelectedUser?.DisplayName ?? "Все сотрудники"}\n\n" +
                   "Возможные причины:\n" +
                   "1. Нет выполненных работ за выбранный период\n" +
                   "2. Статус работ не 'issued'\n" +
                   "3. Нет тарифов в таблице work_rate\n" +
                   "4. Нет записей в таблице production",
            FontSize = 14,
            Foreground = new SolidColorBrush(Color.Parse("#666666")),
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Left,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20)
        };
        stackPanel.Children.Add(description);

        var checkButton = new Button
        {
            Content = "Проверить базу данных",
            HorizontalAlignment = HorizontalAlignment.Center,
            Padding = new Thickness(20, 10),
            Margin = new Thickness(0, 0, 0, 10)
        };

        checkButton.Click += async (s, e) => await CheckDatabaseStatus();
        stackPanel.Children.Add(checkButton);

        SalaryContent?.Children.Add(stackPanel);
    }

    private async Task CheckDatabaseStatus()
    {
        try
        {
            Console.WriteLine("=== ПРОВЕРКА БАЗЫ ДАННЫХ ===");

            using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();

            Console.WriteLine($"Период: {PeriodStart.DateTime:yyyy-MM-dd} - {PeriodEnd.DateTime:yyyy-MM-dd}");

            using (var cmd = new NpgsqlCommand(@"
                SELECT 
                    COUNT(*) as total,
                    COUNT(CASE WHEN status = 'issued' THEN 1 END) as issued,
                    COUNT(CASE WHEN status != 'issued' THEN 1 END) as other_status
                FROM public.production 
                WHERE production_date BETWEEN @startDate AND @endDate",
                connection))
            {
                cmd.Parameters.AddWithValue("@startDate", PeriodStart.DateTime.Date);
                cmd.Parameters.AddWithValue("@endDate", PeriodEnd.DateTime.Date.AddDays(1).AddSeconds(-1));

                using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    Console.WriteLine($"Записи в production:");
                    Console.WriteLine($"  Всего: {reader.GetInt64(0)}");
                    Console.WriteLine($"  Со статусом 'issued': {reader.GetInt64(1)}");
                    Console.WriteLine($"  С другим статусом: {reader.GetInt64(2)}");
                }
            }

            Console.WriteLine("\nТарифы в work_rate:");
            using (var cmd = new NpgsqlCommand(
                "SELECT work_type, rate, use_tonnage, coefficient FROM public.work_rate WHERE is_active = true",
                connection))
            {
                using var reader = await cmd.ExecuteReaderAsync();
                int count = 0;
                while (await reader.ReadAsync())
                {
                    count++;
                    Console.WriteLine($"  {count}. {reader.GetString(0)}: {reader.GetDecimal(1)} руб, " +
                                     $"тоннаж: {reader.GetBoolean(2)}, коэф: {reader.GetDecimal(3)}");
                }
                if (count == 0) Console.WriteLine("  (нет активных тарифов)");
            }

            Console.WriteLine("\nОперации в operation:");
            using (var cmd = new NpgsqlCommand(
                "SELECT id, name, price, unit FROM public.operation",
                connection))
            {
                using var reader = await cmd.ExecuteReaderAsync();
                int count = 0;
                while (await reader.ReadAsync())
                {
                    count++;
                    Console.WriteLine($"  {count}. {reader.GetString(1)}: {reader.GetDecimal(2)} руб, " +
                                     $"ед: {reader.GetString(3)}");
                }
            }

            Console.WriteLine("\nСвязь operation <-> work_rate:");
            using (var cmd = new NpgsqlCommand(@"
                SELECT 
                    o.name as operation_name,
                    o.price as operation_price,
                    o.unit as operation_unit,
                    wr.work_type as rate_type,
                    wr.rate as work_rate,
                    wr.use_tonnage,
                    wr.coefficient
                FROM public.operation o
                LEFT JOIN public.work_rate wr ON LOWER(TRIM(wr.work_type)) = LOWER(TRIM(o.name))
                ORDER BY o.name",
                connection))
            {
                using var reader = await cmd.ExecuteReaderAsync();
                int count = 0;
                while (await reader.ReadAsync())
                {
                    count++;
                    var operationName = reader.GetString(0);
                    var operationPrice = reader.GetDecimal(1);
                    var operationUnit = reader.GetString(2);
                    var hasRate = !reader.IsDBNull(3);

                    if (hasRate)
                    {
                        var rateType = reader.GetString(3);
                        var workRate = reader.GetDecimal(4);
                        var useTonnage = reader.GetBoolean(5);
                        var coefficient = reader.GetDecimal(6);

                        Console.WriteLine($"  {count}. {operationName} ({operationUnit}):");
                        Console.WriteLine($"       Цена из operation: {operationPrice} руб");
                        Console.WriteLine($"       Тариф из work_rate: {workRate} руб");
                        Console.WriteLine($"       Исп.тоннаж: {useTonnage}, Коэф: {coefficient}");
                    }
                    else
                    {
                        Console.WriteLine($"  {count}. {operationName} ({operationUnit}):");
                        Console.WriteLine($"       Цена из operation: {operationPrice} руб");
                        Console.WriteLine($"       (нет тарифа в work_rate)");
                    }
                }
            }

            Console.WriteLine("\n=== ПРОВЕРКА ЗАВЕРШЕНА ===");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка проверки БД: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");
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

            if (SelectedUser != null && SelectedUser.Id > 0)
            {
                sql += " AND u.id = @userId";
            }

            sql += @" 
                GROUP BY u.id, u.first_name, u.last_name, u.middle_name, u.base_salary
                ORDER BY u.last_name, u.first_name
            ";

            using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@startDate", PeriodStart.DateTime);
            command.Parameters.AddWithValue("@endDate", PeriodEnd.DateTime);

            if (SelectedUser != null && SelectedUser.Id > 0)
            {
                command.Parameters.AddWithValue("@userId", SelectedUser.Id);
            }

            using var reader = await command.ExecuteReaderAsync();
            Loges.LoggingProcess(LogLevel.INFO, "Отчет сгенерирован");
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

public class UserComboBoxItem(double id, string firstName, string lastName, string middleName, string login)
{
    public double Id { get; set; } = id;
    public string DisplayName { get; set; } = id == 0 ? "Все сотрудники" : $"{lastName} {firstName} {middleName} ({login})";
}