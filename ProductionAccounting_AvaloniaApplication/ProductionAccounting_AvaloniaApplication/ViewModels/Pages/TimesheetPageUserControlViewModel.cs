using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using static ProductionAccounting_AvaloniaApplication.ViewModels.Control.NotFoundUserControlViewModel;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class TimesheetPageUserControlViewModel : ViewModelBase
{

    public TimesheetPageUserControlViewModel()
    {
        SelectedMassStatus = Statuses[0];
        SelectedSingleStatus = Statuses[0];

        _ = LoadListUsersAsync();
    }

    public StackPanel? CartTimesheet { get; set; }

    private readonly List<CartTimesheetUserControl> _timesheetList = [];

    private DateTime _filterStartDate = DateTime.Today;
    [UsedImplicitly]
    public DateTime FilterStartDate
    {
        get => _filterStartDate;
        set
        {
            if (_filterStartDate != value)
            {
                this.RaiseAndSetIfChanged(ref _filterStartDate, value);
            }
        }
    }

    private DateTime _filterEndDate = DateTime.Today;
    [UsedImplicitly]
    public DateTime FilterEndDate
    {
        get => _filterEndDate;
        set
        {
            if (_filterEndDate != value)
            {
                this.RaiseAndSetIfChanged(ref _filterEndDate, value);
            }
        }
    }

    private DateTime _selectedDate = DateTime.Today;
    [UsedImplicitly]
    public DateTime SelectedDate
    {
        get => _selectedDate;
        set => this.RaiseAndSetIfChanged(ref _selectedDate, value);
    }

    private TimesheetStatus? _selectedSingleStatus;
    [UsedImplicitly]
    public TimesheetStatus? SelectedSingleStatus
    {
        get => _selectedSingleStatus;
        set => this.RaiseAndSetIfChanged(ref _selectedSingleStatus, value);
    }

    private ComboBoxUser? _selectedSingleUser;
    [UsedImplicitly]
    public ComboBoxUser? SelectedSingleUser
    {
        get => _selectedSingleUser;
        set => this.RaiseAndSetIfChanged(ref _selectedSingleUser, value);
    }

    private decimal _singleHours = 8.0m;
    [UsedImplicitly]
    public decimal SingleHours
    {
        get => _singleHours;
        set => this.RaiseAndSetIfChanged(ref _singleHours, value);
    }

    private string _singleNotes = string.Empty;
    [UsedImplicitly]
    public string SingleNotes
    {
        get => _singleNotes;
        set => this.RaiseAndSetIfChanged(ref _singleNotes, value);
    }

    private DateTime _massEditStartDate = DateTime.Today;
    [UsedImplicitly]
    public DateTime MassEditStartDate
    {
        get => _massEditStartDate;
        set => this.RaiseAndSetIfChanged(ref _massEditStartDate, value);
    }

    private DateTime _massEditEndDate = DateTime.Today;
    [UsedImplicitly]
    public DateTime MassEditEndDate
    {
        get => _massEditEndDate;
        set => this.RaiseAndSetIfChanged(ref _massEditEndDate, value);
    }

    [UsedImplicitly]
    public List<TimesheetStatus> Statuses { get; } =
    [
        new TimesheetStatus { Code = "Я", Name = "Явка", Description = "Рабочий день", Color = "#28a745" },
        new TimesheetStatus { Code = "Н", Name = "Неявка", Description = "Неявка по невыясненным причинам", Color = "#dc3545" },
        new TimesheetStatus { Code = "ОТ", Name = "Отпуск", Description = "Оплачиваемый отпуск", Color = "#17a2b8" },
        new TimesheetStatus { Code = "Б", Name = "Больничный", Description = "Больничный лист", Color = "#ffc107" },
        new TimesheetStatus { Code = "В", Name = "Выходной", Description = "Выходной день", Color = "#6c757d" }
    ];

    private TimesheetStatus? _selectedMassStatus;
    [UsedImplicitly]
    public TimesheetStatus? SelectedMassStatus
    {
        get => _selectedMassStatus;
        set => this.RaiseAndSetIfChanged(ref _selectedMassStatus, value);
    }

    private ObservableCollection<ComboBoxUser> _comboBoxUsers = [];
    [UsedImplicitly]
    public ObservableCollection<ComboBoxUser> ComboBoxUsers
    {
        get => _comboBoxUsers;
        set => this.RaiseAndSetIfChanged(ref _comboBoxUsers, value);
    }

    private ComboBoxUser? _userSelectionStates;
    [UsedImplicitly]
    public ComboBoxUser? UserSelectionStates
    {
        get => _userSelectionStates;
        set => this.RaiseAndSetIfChanged(ref _userSelectionStates, value);
    }

    private bool _applyToAllUsers = true;
    [UsedImplicitly]
    public bool ApplyToAllUsers
    {
        get => _applyToAllUsers;
        set
        {
            this.RaiseAndSetIfChanged(ref _applyToAllUsers, value);
            if (value) ApplyToSelectedUsers = false;
        }
    }

    private bool _applyToSelectedUsers;
    [UsedImplicitly]
    public bool ApplyToSelectedUsers
    {
        get => _applyToSelectedUsers;
        set
        {
            this.RaiseAndSetIfChanged(ref _applyToSelectedUsers, value);
            if (value) ApplyToAllUsers = false;
        }
    }

    private bool _excludeWeekends = true;
    [UsedImplicitly]
    public bool ExcludeWeekends
    {
        get => _excludeWeekends;
        set => this.RaiseAndSetIfChanged(ref _excludeWeekends, value);
    }

    private decimal _massEditHours = 8.0m;
    [UsedImplicitly]
    public decimal MassEditHours
    {
        get => _massEditHours;
        set => this.RaiseAndSetIfChanged(ref _massEditHours, value);
    }

    public ICommand RefreshTimesheetCommand
        => new RelayCommand(async void () =>
        {
            try
            {
                await RefreshTimesheetAsync();
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.Critical,
                    ex: ex, 
                    message: "Error refresh timesheet");
            }
        });

    public ICommand ApplyMassEditCommand
        => new RelayCommand(async void () =>
        {
            try
            {
                await ApplyMassEditAsync();
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.Critical,
                    ex: ex, 
                    message: "Error save mass record");
            }
        });

    public ICommand SaveSingleRecordCommand
        => new RelayCommand(async void () =>
        {
            try
            {
                await SaveSingleRecordAsync();
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.Critical,
                    ex: ex, 
                    message: "Error save single record");
            }
        });

    private async Task RefreshTimesheetAsync()
    {
        try
        {
            if (FilterStartDate > FilterEndDate)
            {
                var temp = FilterStartDate;
                _filterStartDate = FilterEndDate;
                _filterEndDate = temp;

                this.RaisePropertyChanged(nameof(FilterStartDate));
                this.RaisePropertyChanged(nameof(FilterEndDate));
            }

            await LoadTimesheetAsync(FilterStartDate, FilterEndDate);

            Loges.LoggingProcess(LogLevel.Info,
                message: $"Табель обновлен за период: {FilterStartDate:dd.MM.yyyy} - {FilterEndDate:dd.MM.yyyy}");
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.Error,
                ex: ex,
                message: "Ошибка обновления табеля");
        }
    }

    private async Task LoadListUsersAsync()
    {
        if (!Internet.ConnectToDataBase())
        {
            Loges.LoggingProcess(LogLevel.Error,
                message: "No connect to db");
            return;
        }

        try
        {
            ComboBoxUsers.Clear();

            const string sqlUsers = "SELECT id, first_name, last_name, middle_name, login FROM public.user WHERE is_active = true ORDER BY last_name, first_name";

            await using (var connection = new NpgsqlConnection(Arguments.Connection))
            {
                await connection.OpenAsync();

                await using var command = new NpgsqlCommand(sqlUsers, connection);
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var user = new ComboBoxUser(
                        reader.GetDouble(0),
                        reader.GetString(1),
                        reader.GetString(2),
                        reader.GetString(3),
                        reader.GetString(4)
                    );

                    ComboBoxUsers.Add(user);
                }
            }

            this.RaisePropertyChanged(nameof(UserSelectionStates));
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
    }

    private async Task SaveSingleRecordAsync()
    {
        if (SelectedSingleUser == null || SelectedSingleStatus == null) return;

        try
        {
            const string sql = "INSERT INTO public.timesheet  (user_id, work_date, status, hours_worked, notes, created_by, created_at)" +
                "VALUES (@userId, @workDate, @status, @hoursWorked, @notes, @createdBy, @createdAt) " +
                "ON CONFLICT (user_id, work_date) " +
                "DO UPDATE SET status = EXCLUDED.status, hours_worked = EXCLUDED.hours_worked, notes = EXCLUDED.notes, updated_by = @createdBy, updated_at = @createdAt";

            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@userId", SelectedSingleUser.Id);
            command.Parameters.AddWithValue("@workDate", SelectedDate);
            command.Parameters.AddWithValue("@status", SelectedSingleStatus.Code);
            command.Parameters.AddWithValue("@hoursWorked", SelectedSingleStatus.Code == "Я" ? SingleHours : 0);
            command.Parameters.AddWithValue("@notes", SingleNotes);
            command.Parameters.AddWithValue("@createdBy", ManagerCookie.GetIdUser ?? 0);
            command.Parameters.AddWithValue("@createdAt", DateTime.Now);

            await command.ExecuteNonQueryAsync();

            ClearSingleForm();

            await RefreshTimesheetAsync();
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.Error,
                ex: ex,
                message: "Ошибка сохранения единичной записи табеля");
        }
    }

    private async Task ApplyMassEditAsync()
    {
        if (!Internet.ConnectToDataBase())
        {
            Loges.LoggingProcess(LogLevel.Error,
                message: "No connect to db");
            return;
        }

        if (SelectedMassStatus == null) return;

        var usersToProcess = ApplyToAllUsers ? [.. ComboBoxUsers] : (SelectedSingleUser != null ? new List<ComboBoxUser> { SelectedSingleUser } : []);

        if (usersToProcess.Count == 0) return;

        var startDate = MassEditStartDate.Date;
        var endDate = MassEditEndDate.Date;
        if (startDate > endDate) return;

        var totalInserted = 0;

        await using var connection = new NpgsqlConnection(Arguments.Connection);
        await connection.OpenAsync();
        await using var transaction = await connection.BeginTransactionAsync();
        try
        {
            const string sql = @"INSERT INTO public.timesheet (user_id, work_date, status, hours_worked, notes, created_by)" +
            "VALUES (@userId, @workDate, @status, @hoursWorked, @notes, @createdBy) ON CONFLICT (user_id, work_date) DO NOTHING";

            await using var command = new NpgsqlCommand(sql, connection, transaction);

            foreach (var user in usersToProcess)
            {
                var currentDate = startDate;
                while (currentDate <= endDate)
                {
                    if (ExcludeWeekends && (currentDate.DayOfWeek == DayOfWeek.Saturday || currentDate.DayOfWeek == DayOfWeek.Sunday))
                    {
                        currentDate = currentDate.AddDays(1);
                        continue;
                    }

                    command.Parameters.Clear();
                    command.Parameters.AddWithValue("@userId", user.Id);
                    command.Parameters.AddWithValue("@workDate", currentDate);
                    command.Parameters.AddWithValue("@status", SelectedMassStatus.Code);
                    command.Parameters.AddWithValue("@hoursWorked", SelectedMassStatus.Code == "Я" ? MassEditHours : 0m);
                    command.Parameters.AddWithValue("@notes", $"Массовое добавление: {SelectedMassStatus.Name}");
                    command.Parameters.AddWithValue("@createdBy", ManagerCookie.GetIdUser ?? 0);

                    int affected = await command.ExecuteNonQueryAsync();
                    if (affected > 0) totalInserted++;

                    currentDate = currentDate.AddDays(1);
                }
            }

            await transaction.CommitAsync();
            Loges.LoggingProcess(LogLevel.Info,
                message: $"Успешно добавлено {totalInserted} новых записей в табель");

            await RefreshTimesheetAsync();
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Loges.LoggingProcess(LogLevel.Error,
                ex: ex,
                message: "Ошибка при массовом добавлении");
        }
    }

    private void ClearSingleForm()
    {
        SelectedSingleUser = null;
        SelectedSingleStatus = Statuses[0];
        SingleHours = 8.0m;
        SingleNotes = string.Empty;
    }

    public async Task LoadTimesheetAsync(DateTime startDate, DateTime endDate)
    {
        try
        {
            StackPanelHelper.ClearAndRefreshStackPanel(CartTimesheet, _timesheetList);

            const string sql = "SELECT t.status, t.notes, t.hours_worked,t.user_id, u.login as user_login FROM public.timesheet t LEFT JOIN public.user u ON t.user_id = u.id WHERE t.work_date BETWEEN @startDate AND @endDate ORDER BY t.work_date DESC, u.login";

            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();

            await using (var command = new NpgsqlCommand(sql, connection))
            {
                command.Parameters.AddWithValue("@startDate", startDate.Date);
                command.Parameters.AddWithValue("@endDate", endDate.Date);

                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var viewModel = new CartTimesheetUserControlViewModel()
                    {
                        Status = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                        Notes = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                        HoursWorked = reader.GetDecimal(2),
                        Login = reader.IsDBNull(4) ? string.Empty : reader.GetString(4)
                    };

                    var cartUser = new CartTimesheetUserControl()
                    {
                        DataContext = viewModel
                    };

                    _timesheetList.Add(cartUser);
                }
            }

            StackPanelHelper.RefreshStackPanelContent(CartTimesheet, _timesheetList);

            if (_timesheetList.Count == 0)
                ItemNotFoundException.Show(CartTimesheet, ErrorLevel.NotFound);
            else
                Loges.LoggingProcess(LogLevel.Info,
                    message: $"Загружено {_timesheetList.Count} записей за период: {startDate:dd.MM.yyyy} - {endDate:dd.MM.yyyy}");
        }
        catch (NpgsqlException ex)
        {
            Loges.LoggingProcess(LogLevel.Error,
                "Connection or request error",
                ex: ex);

            ItemNotFoundException.Show(CartTimesheet, ErrorLevel.NoConnectToDb);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
    }
}
