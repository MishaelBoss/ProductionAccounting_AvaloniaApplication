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

    // Изменяем тип на UserComboBoxItem
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
                        SelectedUser = users[0]; // Устанавливаем по умолчанию "Все сотрудники"
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
                -- Определяем ставку: сначала из work_rate, потом из operation.price
                COALESCE(wr.rate, o.price) AS rate,
                -- Определяем использование тоннажа: из work_rate или по единице измерения
                COALESCE(wr.use_tonnage, LOWER(o.unit) IN ('кг', 'kg', 'тонна', 'ton', 'т')) AS use_tonnage,
                COALESCE(wr.coefficient, 1.0) AS coefficient,
                -- Формула расчета
                CASE 
                    WHEN COALESCE(wr.use_tonnage, LOWER(o.unit) IN ('кг', 'kg', 'тонна', 'ton', 'т')) THEN 
                        COALESCE(wr.rate, o.price) * COALESCE(p.tonnage, 1) / 1000 * p.quantity * COALESCE(wr.coefficient, 1.0)
                    ELSE 
                        COALESCE(wr.rate, o.price) * p.quantity
                END AS calculated_amount,
                -- Формула для отображения
                CASE 
                    WHEN COALESCE(wr.use_tonnage, LOWER(o.unit) IN ('кг', 'kg', 'тонна', 'ton', 'т')) THEN 
                        COALESCE(wr.rate, o.price)::text || ' × ' || COALESCE(p.tonnage, 1)::text || ' / 1000 × ' || 
                        p.quantity::text || ' × ' || COALESCE(wr.coefficient, 1.0)::text || ' = ' ||
                        ROUND(
                            COALESCE(wr.rate, o.price) * COALESCE(p.tonnage, 1) / 1000 * p.quantity * COALESCE(wr.coefficient, 1.0), 
                            2
                        )::text
                    ELSE 
                        COALESCE(wr.rate, o.price)::text || ' × ' || p.quantity::text || ' = ' || 
                        ROUND(COALESCE(wr.rate, o.price) * p.quantity, 2)::text
                END AS calculation_formula
            FROM public.production p
            JOIN public.user u ON u.id = p.user_id
            JOIN public.operation o ON o.id = p.operation_id
            LEFT JOIN public.work_rate wr ON LOWER(TRIM(wr.work_type)) = LOWER(TRIM(o.name))
            WHERE p.production_date BETWEEN @startDate AND @endDate
                AND p.status = 'issued'
                AND u.is_active = true";

            if (SelectedUser != null && SelectedUser.Id > 0)
            {
                sql += " AND p.user_id = @userId";
                parameters.Add(new NpgsqlParameter("@userId", SelectedUser.Id));
            }

            sql += " ORDER BY p.user_id, p.production_date DESC, o.name";

            parameters.Add(new NpgsqlParameter("@startDate", PeriodStart.DateTime.Date));
            parameters.Add(new NpgsqlParameter("@endDate", PeriodEnd.DateTime.Date.AddDays(1).AddSeconds(-1)));

            Console.WriteLine($"SQL запрос: {sql}");
            Console.WriteLine($"Параметры:");
            Console.WriteLine($"  startDate: {PeriodStart.DateTime:yyyy-MM-dd HH:mm:ss}");
            Console.WriteLine($"  endDate: {PeriodEnd.DateTime:yyyy-MM-dd 23:59:59}");

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                Console.WriteLine("Подключение к БД установлено");

                // Сначала проверим, есть ли данные в production за указанный период
                var checkSql = @"
                SELECT COUNT(*) as cnt, 
                       MIN(production_date) as min_date, 
                       MAX(production_date) as max_date
                FROM public.production 
                WHERE production_date BETWEEN @startDate AND @endDate 
                  AND status = 'issued'";

                if (SelectedUser != null && SelectedUser.Id > 0)
                {
                    checkSql += " AND user_id = @userId";
                }

                using (var checkCmd = new NpgsqlCommand(checkSql, connection))
                {
                    checkCmd.Parameters.AddWithValue("@startDate", PeriodStart.DateTime.Date);
                    checkCmd.Parameters.AddWithValue("@endDate", PeriodEnd.DateTime.Date.AddDays(1).AddSeconds(-1));

                    if (SelectedUser != null && SelectedUser.Id > 0)
                    {
                        checkCmd.Parameters.AddWithValue("@userId", SelectedUser.Id);
                    }

                    using (var checkReader = await checkCmd.ExecuteReaderAsync())
                    {
                        if (await checkReader.ReadAsync())
                        {
                            var count = checkReader.GetInt64(0);
                            var minDate = checkReader.IsDBNull(1) ? (DateTime?)null : checkReader.GetDateTime(1);
                            var maxDate = checkReader.IsDBNull(2) ? (DateTime?)null : checkReader.GetDateTime(2);

                            Console.WriteLine($"Проверка production:");
                            Console.WriteLine($"  Всего записей: {count}");
                            Console.WriteLine($"  Минимальная дата: {(minDate?.ToString("yyyy-MM-dd") ?? "нет данных")}");
                            Console.WriteLine($"  Максимальная дата: {(maxDate?.ToString("yyyy-MM-dd") ?? "нет данных")}");

                            if (count == 0)
                            {
                                ShowDetailedNoDataMessage();
                                return;
                            }
                        }
                    }
                }

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(param);
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        Console.WriteLine("Чтение данных из БД...");

                        decimal totalAmount = 0;
                        var userSummaries = new Dictionary<double, UserSalarySummary>();
                        int recordCount = 0;

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
                                var tonnage = reader.IsDBNull(5) ? 1 : reader.GetDecimal(5); // По умолчанию 1, если NULL
                                var notes = reader.IsDBNull(6) ? "" : reader.GetString(6);
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
                                Console.WriteLine($"  Ставка: {rate}");
                                Console.WriteLine($"  Исп.тоннаж: {useTonnage}");
                                Console.WriteLine($"  Коэффициент: {coefficient}");
                                Console.WriteLine($"  Сумма: {amount:N2} руб");
                                Console.WriteLine($"  Формула: {calculationFormula}");

                                totalAmount += amount;

                                if (!userSummaries.ContainsKey(userId))
                                {
                                    userSummaries[userId] = new UserSalarySummary
                                    {
                                        UserId = userId,
                                        EmployeeName = employeeName,
                                        TotalAmount = 0
                                    };
                                }
                                userSummaries[userId].TotalAmount += amount;

                                var viewModel = new SalaryRecordUserControlViewModel
                                {
                                    EmployeeName = employeeName,
                                    OperationName = operationName,
                                    ProductionDate = productionDate,
                                    Quantity = quantity,
                                    Tonnage = tonnage,
                                    Amount = amount,
                                    CalculationFormula = calculationFormula,
                                    Rate = rate,
                                    UseTonnage = useTonnage,
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
                                Console.WriteLine($"Ошибка чтения записи #{recordCount}: {ex.Message}");
                                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                            }
                        }

                        Console.WriteLine($"=== ИТОГО ===");
                        Console.WriteLine($"Всего найдено записей: {recordCount}");
                        Console.WriteLine($"Общая сумма: {totalAmount:N2} руб");
                        Console.WriteLine($"Количество сотрудников: {userSummaries.Count}");

                        // Добавляем итоги по сотрудникам
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

                        // Добавляем общий итог
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
                    }
                }
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
            Console.WriteLine($"=== ОШИБКА ===");
            Console.WriteLine($"Сообщение: {ex.Message}");
            Console.WriteLine($"StackTrace: {ex.StackTrace}");

            if (ex.InnerException != null)
            {
                Console.WriteLine($"Внутренняя ошибка: {ex.InnerException.Message}");
            }

            Loges.LoggingProcess(LogLevel.ERROR, "Ошибка расчета зарплаты", ex: ex);
            StackPanelHelper.ClearAndRefreshStackPanel<SalaryRecordUserControl>(SalaryContent, salaryList);
            ShowDetailedNoDataMessage();
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

        // Иконка
        stackPanel.Children.Add(new TextBlock
        {
            Text = "📊",
            FontSize = 48,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20)
        });

        // Заголовок
        stackPanel.Children.Add(new TextBlock
        {
            Text = "Данные не найдены",
            FontSize = 18,
            FontWeight = FontWeight.Bold,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        });

        // Описание
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

        // Кнопка для проверки БД
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

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                // 1. Проверяем work_rate
                using (var cmd1 = new NpgsqlCommand("SELECT COUNT(*) FROM public.work_rate", connection))
                {
                    var count = await cmd1.ExecuteScalarAsync();
                    Console.WriteLine($"Тарифов в work_rate: {count}");
                }

                // 2. Проверяем production
                using (var cmd2 = new NpgsqlCommand("SELECT COUNT(*) FROM public.production WHERE status = 'issued'", connection))
                {
                    var count = await cmd2.ExecuteScalarAsync();
                    Console.WriteLine($"Записей в production (issued): {count}");
                }

                // 3. Проверяем operation
                using (var cmd3 = new NpgsqlCommand("SELECT id, name, unit FROM public.operation LIMIT 10", connection))
                {
                    using (var reader = await cmd3.ExecuteReaderAsync())
                    {
                        Console.WriteLine("Операции в operation:");
                        while (await reader.ReadAsync())
                        {
                            Console.WriteLine($"  ID: {reader.GetDouble(0)}, Название: {reader.GetString(1)}, Единица: {reader.GetString(2)}");
                        }
                    }
                }

                Console.WriteLine("=== ПРОВЕРКА ЗАВЕРШЕНА ===");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка проверки БД: {ex.Message}");
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

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@startDate", PeriodStart.DateTime);
                    command.Parameters.AddWithValue("@endDate", PeriodEnd.DateTime);

                    if (SelectedUser != null && SelectedUser.Id > 0)
                    {
                        command.Parameters.AddWithValue("@userId", SelectedUser.Id);
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