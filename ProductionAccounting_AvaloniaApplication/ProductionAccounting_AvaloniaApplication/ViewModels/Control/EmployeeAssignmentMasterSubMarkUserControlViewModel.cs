using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using ProductionAccounting_AvaloniaApplication.Scripts;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class EmployeeAssignmentMasterSubMarkUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
    public EmployeeAssignmentMasterSubMarkUserControlViewModel(double id, double subProductId)
    {
        Id = id;
        SubProductId = subProductId;

        _ = LoadListUsersAsync();
    }

    public double Id { get; }
    public double SubProductId { get; }

    public ObservableCollection<ComboBoxUser> Employees { get; } = [];

    private ComboBoxUser? _selectedEmployee;
    public ComboBoxUser? SelectedEmployee
    {
        get => _selectedEmployee;
        set 
        {
            if (_selectedEmployee != value) 
            {
                _selectedEmployee = value;
                OnPropertyChanged(nameof(SelectedEmployee));
                OnPropertyChanged(nameof(CanAssign));
            }
        }
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

                    WeakReferenceMessenger.Default.Send(new RefreshSubProductOperationsMessage(SubProductId));
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

            string sqlUsers = @"
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


    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
