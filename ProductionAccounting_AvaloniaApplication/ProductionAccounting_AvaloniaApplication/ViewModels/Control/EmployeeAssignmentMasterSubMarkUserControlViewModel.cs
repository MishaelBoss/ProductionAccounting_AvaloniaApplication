using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class EmployeeAssignmentMasterSubMarkUserControlViewModel : ViewModelBase
{
    public EmployeeAssignmentMasterSubMarkUserControlViewModel(double id)
    {
        Id = id;

        _ = LoadListUsersAsync();
    }

    public double Id { get; }

    public ObservableCollection<ComboBoxUser> Employees { get; } = [];

    private ComboBoxUser? _selectedEmployee;
    public ComboBoxUser? SelectedEmployee
    {
        get => _selectedEmployee;
        set => this.RaiseAndSetIfChanged(ref _selectedEmployee, value);
    }

    private string? _notes = string.Empty;
    public string? Notes
    {
        get => _notes;
        set => this.RaiseAndSetIfChanged(ref _notes, value);
    }

    public bool CanAssign
        =>  SelectedEmployee != null;

    public ICommand AssignCommand 
        => new RelayCommand(async () => await AssignAsync());

    public ICommand CancelCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseEmployeeAssignmentMasterSubMarkStatusMessage(false)));

    private async Task AssignAsync()
    {
        try
        {
            using (var connection = new NpgsqlConnection(Arguments.connection)) 
            {
                await connection.OpenAsync();

                string sql = "UPDATE public.sub_product_operations SET assigned_to_user_id = @assigned_to_user_id WHERE id = @id";

                using (var command = new NpgsqlCommand(sql, connection)) 
                {
                    command.Parameters.AddWithValue("@assigned_to_user_id", SelectedEmployee!.Id);
                    command.Parameters.AddWithValue("id", Id);

                    await command.ExecuteNonQueryAsync();

                    WeakReferenceMessenger.Default.Send(new RefreshSubProductOperationsMessage(Id));
                    WeakReferenceMessenger.Default.Send(new OpenOrCloseEmployeeAssignmentMasterSubMarkStatusMessage(false));
                }
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.ERROR, ex: ex);
        }
    }

    public async Task LoadListUsersAsync()
    {
        try
        {
            Employees.Clear();

            string sqlUsers = @"SELECT id, first_name, last_name, middle_name, login FROM public.user WHERE is_active = true ORDER BY last_name, first_name";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(sqlUsers, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
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
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);
        }
    }
}
