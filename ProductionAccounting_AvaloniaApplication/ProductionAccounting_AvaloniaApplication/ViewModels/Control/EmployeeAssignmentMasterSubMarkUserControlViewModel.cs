using CommunityToolkit.Mvvm.Input;
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
    public double ProductId { get; }

    public ObservableCollection<ComboBoxUser> Employees { get; } = new();

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

    public bool CanAssign 
        => SelectedEmployee != null &&
           Quantity > 0;

    public ICommand AssignCommand 
        => new RelayCommand(async () => await AssignAsync());

    public EmployeeAssignmentMasterSubMarkUserControlViewModel(double productId)
    {
        ProductId = productId;

        _ = LoadDataAsync();
    }

    private async Task AssignAsync()
    {
        try
        {
            using var conn = new NpgsqlConnection(Arguments.connection);
            await conn.OpenAsync();

            string sql = @"
                INSERT INTO public.task_assignments 
                (sub_product_id, user_id, assigned_quantity, assigned_by, status)
                VALUES (@sub_id, @user_id, @qty, @by, 'assigned')";

            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@sub_id", ProductId);
            cmd.Parameters.AddWithValue("@user_id", SelectedEmployee!.Id);
            cmd.Parameters.AddWithValue("@qty", Quantity);
            cmd.Parameters.AddWithValue("@by", ManagerCookie.GetIdUser ?? 0);

            await cmd.ExecuteNonQueryAsync();
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.ERROR, ex: ex);
        }
    }

    private async Task LoadDataAsync()
    {
        // Загружаешь подмарки и сотрудников
    }
}
