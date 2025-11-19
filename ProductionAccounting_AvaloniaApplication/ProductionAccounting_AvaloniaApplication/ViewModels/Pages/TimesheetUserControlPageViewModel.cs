using CommunityToolkit.Mvvm.Input;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class TimesheetUserControlPageViewModel : ViewModelBase, INotifyPropertyChanging
{
    public TimesheetUserControlPageViewModel() 
    {
        MassEditStartDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        MassEditEndDate = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.DaysInMonth(DateTime.Today.Year, DateTime.Today.Month));

        _ = LoadListUsersAsync();
    }

    private DateTimeOffset _selectedMonth;
    public DateTimeOffset SelectedMonth
    {
        get => _selectedMonth;
        set => this.RaiseAndSetIfChanged(ref _selectedMonth, value);
    }

    private DateTimeOffset _massEditStartDate;
    public DateTimeOffset MassEditStartDate
    {
        get => _massEditStartDate;
        set => this.RaiseAndSetIfChanged(ref _massEditStartDate, value);
    }

    private DateTimeOffset _massEditEndDate;
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

    private ComboBoxShift? _selectedStatus;
    public ComboBoxShift? SelectedStatus
    {
        get => _selectedStatus;
        set
        {
            if (_selectedStatus != value)
            {
                _selectedStatus = value;
                OnPropertyChanged(nameof(SelectedStatus));
            }
        }
    }

    private ObservableCollection<ComboBoxUserAddWork> _comboBoxUsers = [];
    public ObservableCollection<ComboBoxUserAddWork> ComboBoxUsers
    {
        get => _comboBoxUsers;
        set => this.RaiseAndSetIfChanged(ref _comboBoxUsers, value);
    }

    private ComboBoxUserAddWork? _selectedUser;
    public ComboBoxUserAddWork? SelectedUser
    {
        get => _selectedUser;
        set
        {
            if (_selectedUser != value)
            {
                _selectedUser = value;
                OnPropertyChanged(nameof(SelectedUser));
            }
        }
    }

    public ICommand Command
        => new RelayCommand(() => { });

    public async Task LoadListUsersAsync()
    {
        try
        {
            string sqlUsers = @"SELECT id, first_name, last_name, middle_name, login FROM public.user WHERE is_active = true ORDER BY login";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(sqlUsers, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        ComboBoxUsers.Add(new ComboBoxUserAddWork(
                            reader.GetDouble(0),
                            reader.GetString(1),
                            reader.GetString(2),
                            reader.GetString(3),
                            reader.GetString(4)
                        ));
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

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
