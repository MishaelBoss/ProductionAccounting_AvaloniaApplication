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
    public EmployeeAssignmentMasterSubMarkUserControlViewModel(double productId)
    {
        ProductId = productId;

        _ = LoadListUsersAsync();
    }

    public double ProductId { get; }

    public ObservableCollection<ComboBoxUser> Employees { get; } = [];

    private ComboBoxUser? _selectedEmployee;
    public ComboBoxUser? SelectedEmployee
    {
        get => _selectedEmployee;
        set => this.RaiseAndSetIfChanged(ref _selectedEmployee, value);
    }

    private decimal _quantity = 1;
    public decimal Quantity
    {
        get => _quantity;
        set => this.RaiseAndSetIfChanged(ref _quantity, value);
    }

    private string? _notes = string.Empty;
    public string? Notes
    {
        get => _notes;
        set => this.RaiseAndSetIfChanged(ref _notes, value);
    }

    public bool CanAssign
        =>  SelectedEmployee != null 
            && Quantity > 0;

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

                string sql = "INSERT INTO public.task_assignments  (sub_product_operation_id, user_id, assigned_quantity, assigned_by, status, notes) VALUES (@spo_id, @user_id, @qty, @by, 'assigned', @notes)";

                using (var command = new NpgsqlCommand(sql, connection)) 
                {
                    command.Parameters.AddWithValue("@spo_id", ProductId);
                    command.Parameters.AddWithValue("@user_id", SelectedEmployee!.Id);
                    command.Parameters.AddWithValue("@qty", Quantity);
                    command.Parameters.AddWithValue("@by", ManagerCookie.GetIdUser ?? 0);
                    command.Parameters.AddWithValue("@notes", Notes ?? string.Empty);

                    await command.ExecuteNonQueryAsync();

                    WeakReferenceMessenger.Default.Send(new RefreshSubProductOperationsMessage(ProductId));
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
