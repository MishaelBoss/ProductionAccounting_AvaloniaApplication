using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using JetBrains.Annotations;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class AddSubProductUserControlViewModel : ViewModelBase
{
    private double TaskId { get; set; }
    
    private string _messageerror = string.Empty;
    public string Messageerror
    {
        get => _messageerror;
        set => this.RaiseAndSetIfChanged(ref _messageerror, value);
    }

    private string? _title;
    [UsedImplicitly]
    public string? Title 
    {
        get => _title;
        set => SetAndNotify(ref _title, value);
    }

    private decimal? _quantity;
    [UsedImplicitly]
    public decimal? Quantity
    {
        get => _quantity;
        set => SetAndNotify(ref _quantity, value);
    }

    private decimal? _tonnage;
    [UsedImplicitly]
    public decimal? Tonnage
    {
        get => _tonnage;
        set => SetAndNotify(ref _tonnage, value);
    }

    private string? _notes;
    [UsedImplicitly]
    public string? Notes
    {
        get => _notes;
        set => this.RaiseAndSetIfChanged(ref _notes, value);
    }

    [UsedImplicitly]
    public ObservableCollection<ComboBoxUser> Employees { get; } = [];

    private ComboBoxUser? _selectedEmployee;
    public ComboBoxUser? SelectedEmployee
    {
        get => _selectedEmployee;
        set => SetAndNotify(ref _selectedEmployee, value);
    }

    [UsedImplicitly]
    public ObservableCollection<ComboBoxWorkType> WorkTypes { get; } = [];

    private ComboBoxWorkType? _selectedWorkType;
    public ComboBoxWorkType? SelectedWorkType
    {
        get => _selectedWorkType;
        set => SetAndNotify(ref _selectedWorkType, value);
    }

    public AddSubProductUserControlViewModel(double taskId) 
    {
        TaskId = taskId;

        _ = LoadComboBoxAsynck();
    }

    private bool DesignedFields
        => !string.IsNullOrWhiteSpace(Title)
        && SelectedEmployee != null 
        && SelectedWorkType != null
        && Quantity > 0
        && Tonnage > 0;

    public bool IsActiveConfirmButton
        => DesignedFields;

    public ICommand SaveCurrentSubProductCommand
        => new RelayCommand(async void () =>
        {
            try
            {
                await SaveCurrentSubProductAsync();
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.Critical, 
                    ex: ex, 
                    message: "Error add or update");
            }
        });

    public static ICommand CancelCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseAddSubProductStatusMessage(false)));

    private void SetAndNotify<T>(ref T field, T value)
    {
        this.RaiseAndSetIfChanged(ref field, value);
        this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
    }

    private async Task LoadComboBoxAsynck() 
    {
        try
        {
            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();

            await LoadListUsersAsync(connection);
            await LoadListWorkTypeAsync(connection);
        }
        catch (Exception ex) 
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
    }

    private async Task LoadListUsersAsync(NpgsqlConnection connection)
    {
        try
        {
            Employees.Clear();

            const string sql = @"
                                SELECT 
                                    u.id, 
                                    u.first_name, 
                                    u.last_name, 
                                    u.middle_name, 
                                    u.login 
                                FROM public.user AS u
                                JOIN public.user_to_user_type AS uut ON u.id = uut.user_id
                                JOIN public.user_type AS ut ON uut.user_type_id = ut.id
                                WHERE u.is_active = true AND ut.type_user = 'Сотрудник'
                                ORDER BY u.last_name, u.first_name";

            await using var command = new NpgsqlCommand(sql, connection);
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

                Employees.Add(user);
            }
        }
        catch (PostgresException ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
    }

    private async Task LoadListWorkTypeAsync(NpgsqlConnection connection)
    {
        try
        {
            WorkTypes.Clear();

            const string sql = "SELECT * FROM public.work_type";

            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                var work_type = new ComboBoxWorkType(
                    reader.GetDouble(0),
                    reader.GetString(1),
                    reader.GetDecimal(2),
                    reader.GetString(3),
                    reader.GetDecimal(4)
                );

                WorkTypes.Add(work_type);
            }
        }
        catch (PostgresException ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
    }

    private async Task SaveCurrentSubProductAsync()
    {
        if (!DesignedFields)
        {
            Messageerror = "Не все поля заполнены";
            return;
        }
        
        try
        {
            const string sql = "INSERT INTO public.sub_products (product_task_id, name, quantity, tonnage, notes, work_type_id, assigned_to_user_id) VALUES (@task_id, @name, @quantity, @tonnage, @notes, @work_type_id, @assigned_to_user_id)";

            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@task_id", TaskId);
            command.Parameters.AddWithValue("@name", Title ?? string.Empty);
            command.Parameters.AddWithValue("@quantity", Quantity ?? 1);
            command.Parameters.AddWithValue("@tonnage", Tonnage ?? 1);
            command.Parameters.AddWithValue("@notes", Notes ?? "");
            command.Parameters.AddWithValue("@work_type_id", SelectedWorkType?.Id ?? 0);
            command.Parameters.AddWithValue("@assigned_to_user_id", SelectedEmployee?.Id ?? 0);

            await command.ExecuteNonQueryAsync();

            WeakReferenceMessenger.Default.Send(new RefreshSubProductListMessage());
            WeakReferenceMessenger.Default.Send(new OpenOrCloseAddSubProductStatusMessage(false));

            ClearForm();
        }
        catch (PostgresException ex)
        {
            Messageerror = "Ошибка базы данных";
            
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
            Messageerror = "Неизвестная ошибка";
            Loges.LoggingProcess(LogLevel.Error, 
                ex: ex);
        }
    }

    private void ClearForm() 
    {
        Title = string.Empty;
        Quantity = 0;
        Tonnage = 0;
        Notes = string.Empty;
        SelectedEmployee = null;
        SelectedWorkType = null;
    }
}
