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

public class AddOrderUserControlViewModel : ViewModelBase
{
    private readonly double? _id;
        
    private string _messageerror = string.Empty;
    public string Messageerror
    {
        get => _messageerror;
        set => this.RaiseAndSetIfChanged(ref _messageerror, value);
    }

    private string _name = string.Empty;
    [UsedImplicitly]
    public string Name
    {
        get => _name;
        set
        {
            this.RaiseAndSetIfChanged(ref _name, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    private string _counterparties = string.Empty;
    [UsedImplicitly]
    public string Counterparties
    {
        get => _counterparties;
        set
        {
            this.RaiseAndSetIfChanged(ref _counterparties, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    private string _description = string.Empty;
    [UsedImplicitly]
    public string Description
    {
        get => _description;
        set
        {
            this.RaiseAndSetIfChanged(ref _description, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    private decimal _totalWeight = 0m;
    [UsedImplicitly]
    public decimal TotalWeight
    {
        get => _totalWeight;
        set
        {
            this.RaiseAndSetIfChanged(ref _totalWeight, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    private decimal _coefficient = 1m;
    [UsedImplicitly]
    public decimal Coefficient
    {
        get => _coefficient;
        set
        {
            this.RaiseAndSetIfChanged(ref _coefficient, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    [UsedImplicitly]
    public ObservableCollection<ComboBoxUser> Users { get; } = [];
    
    private ComboBoxUser? _selectedUser;
    [UsedImplicitly]
    public ComboBoxUser? SelectedUser
    {
        get => _selectedUser;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedUser, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    public AddOrderUserControlViewModel(
    double? id = null,
    string? name = null,
    string? counterparties = null,
    string? description = null,
    decimal? totalWeight = null,
    decimal? coefficient = null)
    {
        this._id = id;
        Name = name ?? string.Empty;
        Counterparties = counterparties ?? string.Empty;
        Description = description ?? string.Empty;
        TotalWeight = totalWeight ?? 0m;
        Coefficient = coefficient ?? 1m;

        _ = LoadListUsersAsync();
    }

    public static ICommand CancelCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseOrderStatusMessage(false)));

    public ICommand ConfirmCommand
        => new RelayCommand(async void () =>
        {
            try
            {
                await SaveAsync();
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.Critical, 
                    ex: ex, 
                    message: "Failed to update or add");
                Messageerror = "Failed to update or add";
            }
        });

    private bool DesignedFields
        => !string.IsNullOrWhiteSpace(Name)
        && !string.IsNullOrWhiteSpace(Counterparties)
        && Coefficient >= 0.1m
        && Coefficient <= 1
        && SelectedUser != null;

    public bool IsActiveConfirmButton
        => DesignedFields;

    private async Task LoadListUsersAsync()
    {
        try
        {
            Users.Clear();

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
                                WHERE u.is_active = true AND (ut.type_user = 'Администратор' OR ut.type_user = 'Мастер')
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

                Users.Add(user);
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

    private async Task SaveAsync()
    {
        if(!ManagerCookie.IsUserLoggedIn() || (!ManagerCookie.IsManager && !ManagerCookie.IsAdministrator)) return;
        
        if (!DesignedFields) return;
        
        try
        {
            const string addOrderSql = "INSERT INTO public.order (name, counterparties, description, total_weight, coefficient) " +
                        "VALUES (@name, @counterparties, @description, @total_weight, @coefficient) RETURNING id";
            const string updateOrderSql = "UPDATE public.order " +
                "SET name = @name, counterparties = @counterparties, description = @description, total_weight = @total_weight, coefficient = @coefficient " +
                "WHERE id = @id";
            
            var sql = _id != 0 ? updateOrderSql : addOrderSql;

            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            await using var command = new NpgsqlCommand(sql, connection, transaction);

            try
            {

                command.Parameters.AddWithValue("@name", Name.Trim());
                command.Parameters.AddWithValue("@counterparties", Counterparties.Trim());
                command.Parameters.AddWithValue("@description", string.IsNullOrWhiteSpace(Description) ? (object)DBNull.Value : Description.Trim());
                command.Parameters.AddWithValue("@total_weight", TotalWeight);
                command.Parameters.AddWithValue("@coefficient", Coefficient);

                if (_id == 0)
                {
                    var result = await command.ExecuteScalarAsync();

                    if (result == null || Convert.IsDBNull(result)) return;

                    var newOrderId = Convert.ToDouble(result);

                    await CreateTaskForMasterAsync(connection, transaction, newOrderId);
                }
                else
                {
                    if (_id is null or 0) return;

                    command.Parameters.AddWithValue("@id", _id);
                    await command.ExecuteNonQueryAsync();
                }

                await transaction.CommitAsync();
            }
            catch 
            {
                await transaction.RollbackAsync();
            }
        }
        catch (PostgresException ex)
        {
            Loges.LoggingProcess(
                level: LogLevel.Warning, 
                ex: ex,
                message: $"Error DB (SQLState: {ex.SqlState}): {ex.MessageText}" );
                            
            Loges.LoggingProcess(
                level: LogLevel.Warning, 
                ex: ex,
                message: $"Error DB (Detail: {ex.Detail})" );
            
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
    }

    private async Task CreateTaskForMasterAsync(NpgsqlConnection connection, NpgsqlTransaction transaction, double orderId)
    {
        try
        {
            const string sql = "INSERT INTO public.product_tasks (order_id, created_by, assigned_by) " +
                "VALUES (@oid, @created_by, @assigned_by)";
            
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@oid", orderId);
            command.Parameters.AddWithValue("@created_by", ManagerCookie.GetIdUser ?? 0);
            if (SelectedUser == null || SelectedUser.Id == 0)
            {
                Messageerror = "Не выбран пользователь";
                return;
            }
            command.Parameters.AddWithValue("@assigned_by", SelectedUser.Id);
            
            await command.ExecuteNonQueryAsync();
            await transaction.CommitAsync();


            WeakReferenceMessenger.Default.Send(new RefreshProductListMessage());
            WeakReferenceMessenger.Default.Send(new OpenOrCloseOrderStatusMessage(false));
            ClearForm();
        }
        catch (PostgresException ex)
        {
            await transaction.RollbackAsync();
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            Loges.LoggingProcess(LogLevel.Warning, ex: ex);
        }
    }

    private void ClearForm()
    {
        Name = string.Empty;
        Counterparties = string.Empty;
        Description = string.Empty;
        TotalWeight = 0m;
        Coefficient = 1m;
        SelectedUser = null;
        Messageerror = string.Empty;
    }
}
