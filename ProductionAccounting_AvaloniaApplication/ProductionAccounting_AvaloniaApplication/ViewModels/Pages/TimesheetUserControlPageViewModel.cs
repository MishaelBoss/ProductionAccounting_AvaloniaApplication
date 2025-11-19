using CommunityToolkit.Mvvm.Input;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class TimesheetUserControlPageViewModel : ViewModelBase, INotifyPropertyChanging
{
    public TimesheetUserControlPageViewModel() 
    {
        SelectedMassStatus = Statuses[0];
        SelectedSingleStatus = Statuses[0];

        _ = LoadListUsersAsync();
    }

    private DateTimeOffset _selectedDate = new (DateTime.Today);
    public DateTimeOffset SelectedDate
    {
        get => _selectedDate;
        set => this.RaiseAndSetIfChanged(ref _selectedDate, value);
    }

    private TimesheetStatus _selectedSingleStatus;
    public TimesheetStatus SelectedSingleStatus
    {
        get => _selectedSingleStatus;
        set => this.RaiseAndSetIfChanged(ref _selectedSingleStatus, value);
    }

    private ComboBoxUserAddWork _selectedSingleUser;
    public ComboBoxUserAddWork SelectedSingleUser
    {
        get => _selectedSingleUser;
        set => this.RaiseAndSetIfChanged(ref _selectedSingleUser, value);
    }

    private decimal _singleHours = 8.0m;
    public decimal SingleHours
    {
        get => _singleHours;
        set => this.RaiseAndSetIfChanged(ref _singleHours, value);
    }

    private string _singleNotes = string.Empty;
    public string SingleNotes
    {
        get => _singleNotes;
        set => this.RaiseAndSetIfChanged(ref _singleNotes, value);
    }

    private DateTimeOffset _massEditStartDate = new (DateTime.Today);
    public DateTimeOffset MassEditStartDate
    {
        get => _massEditStartDate;
        set => this.RaiseAndSetIfChanged(ref _massEditStartDate, value);
    }

    private DateTimeOffset _massEditEndDate = new(DateTime.Today);
    public DateTimeOffset MassEditEndDate
    {
        get => _massEditEndDate;
        set => this.RaiseAndSetIfChanged(ref _massEditEndDate, value);
    }

    public List<TimesheetStatus> Statuses { get; } =
    [
        new TimesheetStatus { Code = "Я", Name = "Явка", Description = "Рабочий день", Color = "#28a745" },
        new TimesheetStatus { Code = "Н", Name = "Неявка", Description = "Неявка по невыясненным причинам", Color = "#dc3545" },
        new TimesheetStatus { Code = "ОТ", Name = "Отпуск", Description = "Оплачиваемый отпуск", Color = "#17a2b8" },
        new TimesheetStatus { Code = "Б", Name = "Больничный", Description = "Больничный лист", Color = "#ffc107" },
        new TimesheetStatus { Code = "В", Name = "Выходной", Description = "Выходной день", Color = "#6c757d" }
    ];

    private TimesheetStatus _selectedMassStatus;
    public TimesheetStatus SelectedMassStatus
    {
        get => _selectedMassStatus;
        set => this.RaiseAndSetIfChanged(ref _selectedMassStatus, value);
    }

    private ObservableCollection<ComboBoxUserAddWork> _comboBoxUsers = [];
    public ObservableCollection<ComboBoxUserAddWork> ComboBoxUsers
    {
        get => _comboBoxUsers;
        set => this.RaiseAndSetIfChanged(ref _comboBoxUsers, value);
    }

    private ComboBoxUserAddWork? _userSelectionStates;
    public ComboBoxUserAddWork? UserSelectionStates
    {
        get => _userSelectionStates;
        set
        {
            if (_userSelectionStates != value)
            {
                _userSelectionStates = value;
                OnPropertyChanged(nameof(UserSelectionStates));
            }
        }
    }

    private bool _applyToAllUsers = true;
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
    public bool ExcludeWeekends
    {
        get => _excludeWeekends;
        set => this.RaiseAndSetIfChanged(ref _excludeWeekends, value);
    }

    private decimal _massEditHours = 8.0m;
    public decimal MassEditHours
    {
        get => _massEditHours;
        set => this.RaiseAndSetIfChanged(ref _massEditHours, value);
    }

    public ICommand ApplyMassEditCommand
        => new RelayCommand(async () => await ApplyMassEditAsync());

    public ICommand SaveSingleRecordCommand
        => new RelayCommand(async () => await SaveSingleRecordAsync());

    public async Task LoadListUsersAsync()
    {
        try
        {
            ComboBoxUsers.Clear();

            string sqlUsers = @"SELECT id, first_name, last_name, middle_name, login FROM public.user WHERE is_active = true ORDER BY last_name, first_name";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(sqlUsers, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var user = new ComboBoxUserAddWork(
                            reader.GetDouble(0),
                            reader.GetString(1),
                            reader.GetString(2),
                            reader.GetString(3),
                            reader.GetString(4)
                        );

                        ComboBoxUsers.Add(user);
                    }
                }
            }

            this.RaisePropertyChanged(nameof(UserSelectionStates));
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING, ex: ex);
        }
    }

    public async Task SaveSingleRecordAsync()
    {
        if (SelectedSingleUser == null || SelectedSingleStatus == null) return;

        try
        {
            string sql = @"INSERT INTO public.timesheet (user_id, work_date, status, hours_worked, notes, created_by) VALUES (@userId, @workDate, @status, @hoursWorked, @notes, @createdBy)";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@userId", SelectedSingleUser.Id);
                    command.Parameters.AddWithValue("@workDate", SelectedDate.DateTime);
                    command.Parameters.AddWithValue("@status", SelectedSingleStatus.Code);
                    command.Parameters.AddWithValue("@hoursWorked", SelectedSingleStatus.Code == "Я" ? SingleHours : 0);
                    command.Parameters.AddWithValue("@notes", SingleNotes ?? "");
                    command.Parameters.AddWithValue("@createdBy", ManagerCookie.GetIdUser);

                    int result = await command.ExecuteNonQueryAsync();

                    if (result > 0) ClearSingleForm();
                }
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.ERROR, ex: ex, message: "Ошибка сохранения единичной записи табеля");
        }
    }

    public async Task ApplyMassEditAsync()
    {
        if (SelectedMassStatus == null) return;

        var usersToProcess = ApplyToAllUsers ? ComboBoxUsers.ToList() : (SelectedSingleUser != null ? new List<ComboBoxUserAddWork> { SelectedSingleUser } : new());

        if (!usersToProcess.Any()) return;

        var startDate = MassEditStartDate.DateTime.Date;
        var endDate = MassEditEndDate.DateTime.Date;
        if (startDate > endDate) return;

        int totalInserted = 0;

        await using var connection = new NpgsqlConnection(Arguments.connection);
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
                    command.Parameters.AddWithValue("@createdBy", ManagerCookie.GetIdUser);

                    int affected = await command.ExecuteNonQueryAsync();
                    if (affected > 0) totalInserted++;

                    currentDate = currentDate.AddDays(1);
                }
            }

            await transaction.CommitAsync();
            Loges.LoggingProcess(LogLevel.INFO, 
                message: $"Успешно добавлено {totalInserted} новых записей в табель");
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Loges.LoggingProcess(LogLevel.ERROR, 
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


    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
