using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using ProductionAccounting_AvaloniaApplication.Scripts;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class EmployeeAssignmentMasterSubMarkUserControlViewModel : ViewModelBase
{
    public EmployeeAssignmentMasterSubMarkUserControlViewModel(double id, double subProductId)
    {
        Id = id;
        SubProductId = subProductId;

        _ = LoadListUsersAsync();
    }
    
    private string _messageerror = string.Empty;
    public string Messageerror
    {
        get => _messageerror;
        set => this.RaiseAndSetIfChanged(ref _messageerror, value);
    }

    private double Id { get; }
    private double SubProductId { get; }

    [UsedImplicitly]
    public ObservableCollection<ComboBoxUser> Employees { get; } = [];

    private ComboBoxUser? _selectedEmployee;
    public ComboBoxUser? SelectedEmployee
    {
        get => _selectedEmployee;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedEmployee, value);
            this.RaisePropertyChanged(nameof(CanAssign));
        }
    }

    public bool CanAssign
        =>  SelectedEmployee != null;

    public ICommand AssignCommand 
        => new RelayCommand(async void () =>
        {
            try
            {
                await AssignAsync();
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.Critical, 
                    ex: ex, 
                    message: "Failed to add");
            }
        });

    public static ICommand CancelCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseEmployeeAssignmentMasterSubMarkStatusMessage(false)));

    private async Task AssignAsync()
    {
        if (SelectedEmployee?.Id == 0)
        {
            Messageerror = "Не все поля заполнены";
            return;
        }

        try
        {
            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();

            const string sql = "UPDATE public.sub_product_operations SET assigned_to_user_id = @assigned_to_user_id WHERE id = @id";

            await using var command = new NpgsqlCommand(sql, connection);
            if(SelectedEmployee is null) return;
            command.Parameters.AddWithValue("@assigned_to_user_id", SelectedEmployee.Id);
            command.Parameters.AddWithValue("id", Id);

            await command.ExecuteNonQueryAsync();

            WeakReferenceMessenger.Default.Send(new RefreshSubProductOperationsMessage(SubProductId));
            WeakReferenceMessenger.Default.Send(new OpenOrCloseEmployeeAssignmentMasterSubMarkStatusMessage(false));
        }
        catch (PostgresException ex)
        {
            Messageerror = "Ошибка базы данных";

            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
        catch (Exception ex)
        {
            Messageerror = "Неизвестная ошибка";

            Loges.LoggingProcess(level: LogLevel.Error, 
                ex: ex);
        }
    }

    private async Task LoadListUsersAsync()
    {
        try
        {
            Employees.Clear();

            const string sqlUsers = @"
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

            await using var connection = new NpgsqlConnection(Arguments.Connection);
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
}
