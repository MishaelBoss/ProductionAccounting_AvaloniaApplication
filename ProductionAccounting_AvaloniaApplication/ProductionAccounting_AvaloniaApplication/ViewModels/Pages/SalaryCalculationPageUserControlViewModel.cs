using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class SalaryCalculationPageUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
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
    [UsedImplicitly]
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

    private decimal _grandTotalGross;
    [UsedImplicitly]
    public decimal GrandTotalGross
    {
        get => _grandTotalGross;
        set => this.RaiseAndSetIfChanged(ref _grandTotalGross, value);
    }

    private decimal _grandTotalTax;
    [UsedImplicitly]
    public decimal GrandTotalTax
    {
        get => _grandTotalTax;
        set => this.RaiseAndSetIfChanged(ref _grandTotalTax, value);
    }

    private decimal _grandTotalNet;
    [UsedImplicitly]
    public decimal GrandTotalNet
    {
        get => _grandTotalNet;
        set => this.RaiseAndSetIfChanged(ref _grandTotalNet, value);
    }

    private IStorageProvider? _storageProvider;
    [UsedImplicitly]
    public IStorageProvider? StorageProvider
    {
        get => _storageProvider;
        set => this.RaiseAndSetIfChanged(ref _storageProvider, value);
    }

    public ICommand ExportToPdfCommand
        => new AsyncRelayCommand(ExportToPdfAsync);

    public StackPanel? SalaryContent { get; set; }

    private readonly List<SalaryRecordUserControl> _salaryList = [];

    public ICommand CalculateSalaryCommand
        => new RelayCommand(async void () =>
        {
            try
            {
                await CalculateSalaryAsync();
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.Critical,
                    ex: ex, 
                    message: "Error calculate salary");
            }
        });

    public ICommand LoadUsersCommand
        => new RelayCommand(async void () =>
        {
            try
            {
                await LoadUsersAsync();
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.Critical,
                    ex: ex, 
                    message: "Error load user");
            }
        });

    public SalaryCalculationPageUserControlViewModel()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        
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

            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();
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
            Loges.LoggingProcess(LogLevel.Error, "Ошибка загрузки пользователей", ex: ex);
        }
    }

    private async Task CalculateSalaryAsync()
    {
        Console.WriteLine($"=== НАЧАЛО РАСЧЕТА ЗАРПЛАТЫ ===");
        Console.WriteLine($"Период: {PeriodStart.DateTime:dd.MM.yyyy} - {PeriodEnd.DateTime:dd.MM.yyyy}");
        Console.WriteLine($"Сотрудник: {SelectedUser?.DisplayName ?? "Все"}");

        StackPanelHelper.ClearAndRefreshStackPanel(SalaryContent, _salaryList);

        try
        {
            var parameters = new List<NpgsqlParameter>();
            var sql = @"
                SELECT
                    p.user_id,
                    u.id AS user_id,
                    u.first_name || ' ' || u.last_name || ' ' || COALESCE(u.middle_name, '') AS employee_name,
                    u.base_salary,
                    o.name AS operation_name,
                    p.production_date,
                    p.quantity,
                    p.tonnage,
                    o.unit,
                    COALESCE(wr.rate, o.price) AS rate,
                    COALESCE(wr.use_tonnage,
                        LOWER(o.unit) IN ('кг', 'kg', 'тонна', 'ton', 'т', 'т.', 'тонны', 'килограмм', 'килограммы')) AS use_tonnage,
                    COALESCE(wr.coefficient, 1.0) AS coefficient,
                    CASE
                        WHEN COALESCE(wr.use_tonnage,
                                LOWER(o.unit) IN ('кг', 'kg', 'тонна', 'ton', 'т', 'т.', 'тонны', 'килограмм', 'килограммы'))
                            AND p.tonnage > 0
                        THEN
                            COALESCE(wr.rate, o.price) * p.tonnage / 1000 * p.quantity * COALESCE(wr.coefficient, 1.0)
                        ELSE
                            COALESCE(wr.rate, o.price) * p.quantity
                    END AS calculated_amount,
                    CASE
                        WHEN COALESCE(wr.use_tonnage,
                                LOWER(o.unit) IN ('кг', 'kg', 'тонна', 'ton', 'т', 'т.', 'тонны', 'килограмм', 'килограммы'))
                            AND p.tonnage > 0
                        THEN
                            COALESCE(wr.rate, o.price)::text || ' × ' || p.tonnage::text || ' / 1000 × ' ||
                            p.quantity::text || ' × ' || COALESCE(wr.coefficient, 1.0)::text
                        ELSE
                            COALESCE(wr.rate, o.price)::text || ' × ' || p.quantity::text
                    END || ' = ' ||
                    ROUND(CASE
                        WHEN COALESCE(wr.use_tonnage,
                                LOWER(o.unit) IN ('кг', 'kg', 'тонна', 'ton', 'т', 'т.', 'тонны', 'килограмм', 'килограммы'))
                            AND p.tonnage > 0
                        THEN COALESCE(wr.rate, o.price) * p.tonnage / 1000 * p.quantity * COALESCE(wr.coefficient, 1.0)
                        ELSE COALESCE(wr.rate, o.price) * p.quantity
                    END, 2) AS calculation_formula
                FROM public.production p
                JOIN public.""user"" u ON u.id = p.user_id
                JOIN public.operation o ON o.id = p.operation_id
                LEFT JOIN public.work_rate wr ON LOWER(TRIM(wr.work_type)) = LOWER(TRIM(o.name))
                WHERE p.production_date BETWEEN @startDate AND @endDate
                    AND p.status IN ('issued', 'completed')
                    AND u.is_active = true";
            
            if (SelectedUser is null && SelectedUser is { Id: > 0})
            {
                sql += " AND p.user_id = @userId";
                parameters.Add(new NpgsqlParameter("@userId", SelectedUser.Id));
            }

            sql += " ORDER BY u.last_name, p.production_date DESC, o.name";

            parameters.Add(new NpgsqlParameter("@startDate", PeriodStart.DateTime.Date));
            parameters.Add(new NpgsqlParameter("@endDate", PeriodEnd.DateTime.Date.AddDays(1).AddSeconds(-1)));

            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();

            var userFinalSalary = new Dictionary<double, UserFinalSalarySummary>();

            await using (var command = new NpgsqlCommand(sql, connection))
            {
                foreach (var param in parameters) command.Parameters.Add(param);

                await using var reader = await command.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var userId = reader.GetDouble(0);
                    var employeeName = reader.GetString(2);
                    var baseSalary = reader.IsDBNull(3) ? 0m : reader.GetDecimal(3);
                    var operationName = reader.GetString(4);
                    var productionDate = reader.GetDateTime(5);
                    var quantity = reader.GetDecimal(6);
                    var tonnage = reader.IsDBNull(7) ? 0m : reader.GetDecimal(7);
                    var unit = reader.IsDBNull(8) ? "шт" : reader.GetString(8);
                    var rate = reader.GetDecimal(9);
                    var useTonnage = reader.GetBoolean(10);
                    var coefficient = reader.GetDecimal(11);
                    var amount = reader.GetDecimal(12);
                    var formula = reader.GetString(13);

                    var recordVm = new SalaryRecordUserControlViewModel
                    {
                        EmployeeName = employeeName,
                        OperationName = operationName,
                        ProductionDate = productionDate,
                        Quantity = quantity,
                        Tonnage = tonnage,
                        Amount = amount,
                        CalculationFormula = formula,
                        Unit = unit,
                        Rate = rate,
                        UseTonnage = useTonnage && tonnage > 0,
                        Coefficient = coefficient,
                        IsSummary = false,
                        IsTotal = false
                    };

                    var control = new SalaryRecordUserControl { DataContext = recordVm };
                    _salaryList.Add(control);

                    if (!userFinalSalary.TryGetValue(userId, out var summary))
                    {
                        summary = new UserFinalSalarySummary
                        {
                            UserId = userId,
                            EmployeeName = employeeName,
                            BaseSalary = baseSalary,
                            ProductionAmount = 0m,
                            TotalGross = 0m,
                            TaxNDFL = 0m,
                            NetSalary = 0m
                        };
                        userFinalSalary[userId] = summary;
                    }

                    summary.ProductionAmount += amount;
                }
            }

            decimal grandTotalGross = 0m;
            decimal grandTotalTax = 0m;
            decimal grandTotalNet = 0m;

            foreach (var summary in userFinalSalary.Values)
            {
                summary.TotalGross = summary.BaseSalary + summary.ProductionAmount;
                summary.TaxNDFL = Math.Round(summary.TotalGross * 0.13m, 2);
                summary.NetSalary = summary.TotalGross - summary.TaxNDFL;

                grandTotalGross += summary.TotalGross;
                grandTotalTax += summary.TaxNDFL;
                grandTotalNet += summary.NetSalary;

                var summaryVm = new SalaryRecordUserControlViewModel
                {
                    EmployeeName = $"{summary.EmployeeName} — ИТОГО",
                    BaseSalary = summary.BaseSalary,
                    ProductionAmount = summary.ProductionAmount,
                    TotalGross = summary.TotalGross,
                    TaxNDFL = summary.TaxNDFL,
                    NetSalary = summary.NetSalary,
                    IsSummary = true
                };

                _salaryList.Add(new SalaryRecordUserControl { DataContext = summaryVm });
            }

            GrandTotalGross = grandTotalGross;
            GrandTotalTax = grandTotalTax;
            GrandTotalNet = grandTotalNet;

            if (userFinalSalary.Count > 0)
            {
                var totalVm = new SalaryRecordUserControlViewModel
                {
                    EmployeeName = "ОБЩИЙ ИТОГ",
                    TotalGross = grandTotalGross,
                    TaxNDFL = grandTotalTax,
                    NetSalary = grandTotalNet,
                    IsTotal = true
                };
                _salaryList.Add(new SalaryRecordUserControl { DataContext = totalVm });
            }

            StackPanelHelper.RefreshStackPanelContent(SalaryContent, _salaryList);

            if (_salaryList.Count == 0)
                ShowDetailedNoDataMessage();
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.Error, "Ошибка расчета зарплаты", ex: ex);
            StackPanelHelper.ClearAndRefreshStackPanel(SalaryContent, _salaryList);
            ShowDetailedNoDataMessage();
        }
    }

    private void ShowDetailedNoDataMessage()
    {
        var stackPanel = new StackPanel
        {
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Center,
            Margin = new Thickness(0, 40, 0, 0)
        };

        stackPanel.Children.Add(new Avalonia.Controls.TextBlock
        {
            Text = "📊",
            FontSize = 48,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20)
        });

        stackPanel.Children.Add(new Avalonia.Controls.TextBlock
        {
            Text = "Данные не найдены",
            FontSize = 18,
            FontWeight = Avalonia.Media.FontWeight.Bold,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        });

        var description = new Avalonia.Controls.TextBlock
        {
            Text = $"За период {PeriodStart.DateTime:dd.MM.yyyy} - {PeriodEnd.DateTime:dd.MM.yyyy}\n" +
                $"Сотрудник: {SelectedUser?.DisplayName ?? "Все сотрудники"}\n\n" +
                "Возможные причины:\n" +
                "1. Нет выполненных работ за выбранный период\n" +
                "2. Статус работ не 'issued'\n" +
                "3. Нет тарифов в таблице work_rate\n" +
                "4. Нет записей в таблице production",
            FontSize = 14,
            Foreground = new SolidColorBrush(Avalonia.Media.Color.Parse("#666666")),
            TextWrapping = TextWrapping.Wrap,
            TextAlignment = TextAlignment.Left,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Margin = new Thickness(0, 0, 0, 20)
        };
        stackPanel.Children.Add(description);

        var checkButton = new Avalonia.Controls.Button
        {
            Content = "Проверить базу данных",
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Center,
            Padding = new Thickness(20, 10),
            Margin = new Thickness(0, 0, 0, 10)
        };

        checkButton.Click += async (_, _) => await CheckDatabaseStatus();
        stackPanel.Children.Add(checkButton);

        SalaryContent?.Children.Add(stackPanel);
    }

    private async Task CheckDatabaseStatus()
    {
        try
        {
            Console.WriteLine("=== ПРОВЕРКА БАЗЫ ДАННЫХ ===");

            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();

            Console.WriteLine($"Период: {PeriodStart.DateTime:yyyy-MM-dd} - {PeriodEnd.DateTime:yyyy-MM-dd}");

            await using (var cmd = new NpgsqlCommand(@"
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

                await using var reader = await cmd.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    Console.WriteLine($"Записи в production:");
                    Console.WriteLine($"  Всего: {reader.GetInt64(0)}");
                    Console.WriteLine($"  Со статусом 'issued': {reader.GetInt64(1)}");
                    Console.WriteLine($"  С другим статусом: {reader.GetInt64(2)}");
                }
            }

            Console.WriteLine("\nТарифы в work_rate:");
            await using (var cmd = new NpgsqlCommand(
                "SELECT work_type, rate, use_tonnage, coefficient FROM public.work_rate WHERE is_active = true",
                connection))
            {
                await using var reader = await cmd.ExecuteReaderAsync();
                var count = 0;
                while (await reader.ReadAsync())
                {
                    count++;
                    Console.WriteLine($"  {count}. {reader.GetString(0)}: {reader.GetDecimal(1)} руб, " +
                                     $"тоннаж: {reader.GetBoolean(2)}, коэф: {reader.GetDecimal(3)}");
                }
                if (count == 0) Console.WriteLine("  (нет активных тарифов)");
            }

            Console.WriteLine("\nОперации в operation:");
            await using (var cmd = new NpgsqlCommand(
                "SELECT id, name, price, unit FROM public.operation",
                connection))
            {
                await using var reader = await cmd.ExecuteReaderAsync();
                var count = 0;
                while (await reader.ReadAsync())
                {
                    count++;
                    Console.WriteLine($"  {count}. {reader.GetString(1)}: {reader.GetDecimal(2)} руб, " +
                                     $"ед: {reader.GetString(3)}");
                }
            }

            Console.WriteLine("\nСвязь operation <-> work_rate:");
            await using (var cmd = new NpgsqlCommand(@"
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
                await using var reader = await cmd.ExecuteReaderAsync();
                var count = 0;
                while (await reader.ReadAsync())
                {
                    count++;
                    var operationName = reader.GetString(0);
                    var operationPrice = reader.GetDecimal(1);
                    var operationUnit = reader.GetString(2);
                    var hasRate = !reader.IsDBNull(3);

                    if (hasRate)
                    {
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

    private async Task ExportToPdfAsync()
    {
        Console.WriteLine("=== Кнопка Экспорт в PDF нажата ===");

        if (_salaryList.Count == 0 || StorageProvider == null)
        {
            Console.WriteLine("Нет данных или StorageProvider null");
            return;
        }

        try
        {
            Console.WriteLine("Открытие диалога сохранения...");

            var file = await StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions
            {
                Title = "Сохранить отчёт по зарплате",
                DefaultExtension = "pdf",
                SuggestedFileName = $"Зарплата_{PeriodStart.DateTime:dd-MM-yyyy}_{PeriodEnd.DateTime:dd-MM-yyyy}.pdf"
            });

            if (file == null)
            {
                Console.WriteLine("Пользователь отменил");
                return;
            }

            Console.WriteLine($"Файл выбран: {file.Name}");

            using var memoryStream = new MemoryStream();

            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);
                    page.PageColor(QuestPDF.Helpers.Colors.White);
                    page.DefaultTextStyle(x => x.FontSize(9));

                    page.Header()
                        .Text("Отчёт по расчёту заработной платы")
                        .FontSize(18)
                        .Bold()
                        .AlignCenter();

                    page.Content()
                        .PaddingVertical(15)
                        .Column(column =>
                        {
                            column.Item().Text($"Период: {PeriodStart.DateTime:dd.MM.yyyy} – {PeriodEnd.DateTime:dd.MM.yyyy}");
                            column.Item().Text($"Сотрудник: {SelectedUser?.DisplayName ?? "Все сотрудники"}");
                            column.Item().PaddingBottom(10);

                            column.Item().Text("Детали операций")
                                .FontSize(12)
                                .Bold();

                            column.Item().PaddingBottom(5);

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(140);
                                    columns.ConstantColumn(60);
                                    columns.ConstantColumn(60);
                                    columns.ConstantColumn(70);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Сотрудник / Операция").Bold();
                                    header.Cell().Text("Дата").Bold();
                                    header.Cell().Text("Кол-во").Bold();
                                    header.Cell().Text("Сумма").Bold();
                                });

                                foreach (var record in _salaryList)
                                {
                                    var vm = record.DataContext as SalaryRecordUserControlViewModel;
                                    if (vm == null || vm.IsSummary || vm.IsTotal) continue;

                                    table.Cell().Text(vm.EmployeeName).FontSize(8);
                                    table.Cell().Text(vm.ProductionDate.ToString("dd.MM.yyyy")).FontSize(8);
                                    table.Cell().Text($"{vm.Quantity:N2} {vm.Unit}").FontSize(8);
                                    table.Cell().Text(vm.Amount.ToString("N2") + " ₽").FontSize(8);
                                }
                            });

                            column.Item().PaddingTop(20);
                            column.Item().Text("Итоги по сотрудникам")
                                .FontSize(12)
                                .Bold();

                            column.Item().PaddingBottom(5);

                            column.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(140);
                                    columns.ConstantColumn(60);
                                    columns.ConstantColumn(70);
                                    columns.ConstantColumn(60);
                                    columns.ConstantColumn(60);
                                    columns.ConstantColumn(60);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Сотрудник").Bold();
                                    header.Cell().Text("Оклад").Bold();
                                    header.Cell().Text("Производство").Bold();
                                    header.Cell().Text("Всего").Bold();
                                    header.Cell().Text("НДФЛ").Bold();
                                    header.Cell().Text("К выдаче").Bold();
                                });

                                foreach (var record in _salaryList)
                                {
                                    var vm = record.DataContext as SalaryRecordUserControlViewModel;
                                    if (vm == null || !vm.IsSummary) continue;

                                    table.Cell().Text(vm.EmployeeName).FontSize(8);
                                    table.Cell().Text(vm.BaseSalary.ToString("N2") + " ₽").FontSize(8);
                                    table.Cell().Text(vm.ProductionAmount.ToString("N2") + " ₽").FontSize(8);
                                    table.Cell().Text(vm.TotalGross.ToString("N2") + " ₽").FontSize(8);
                                    table.Cell().Text(vm.TaxNDFL.ToString("N2") + " ₽").FontSize(8);
                                    table.Cell().Text(vm.NetSalary.ToString("N2") + " ₽").FontSize(8);
                                }
                            });

                            column.Item().PaddingTop(20);
                            column.Item().Text("Общий итог")
                                .FontSize(14)
                                .Bold();

                            column.Item().Text($"Всего начислено: {GrandTotalGross:N2} ₽");
                            column.Item().Text($"НДФЛ: {GrandTotalTax:N2} ₽");
                            column.Item().Text($"К выдаче: {GrandTotalNet:N2} ₽")
                                .FontSize(12)
                                .Bold();
                        });

                    page.Footer()
                        .AlignCenter()
                        .Text($"Сгенерировано {DateTime.Now:dd.MM.yyyy HH:mm}")
                        .FontSize(8);
                });
            })
            .GeneratePdf(memoryStream);

            memoryStream.Position = 0;

            await using var fileStream = await file.OpenWriteAsync();
            await memoryStream.CopyToAsync(fileStream);

            Console.WriteLine("PDF успешно сохранён!");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка экспорта PDF: {ex.Message}");
            Loges.LoggingProcess(LogLevel.Error, "Ошибка экспорта PDF", ex: ex);
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