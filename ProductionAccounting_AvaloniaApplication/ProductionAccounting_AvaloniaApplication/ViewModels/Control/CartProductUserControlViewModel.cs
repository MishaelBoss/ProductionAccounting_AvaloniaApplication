using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartProductUserControlViewModel : ViewModelBase
{
    public ICommand OpenPruductViewCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseProductViewStatusMessage(true, ProductName, Id, Mark, Coefficient, Notes, Status)));

    public ICommand CompleteTaskCommand
        => new RelayCommand(async void () =>
        {
            try
            {
                if (!CanCompleteTask) return; await CompleteTaskAndShipAsync();
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.Critical, 
                    ex: ex, 
                    message: "Error complete task");
            }
        });

    public ICommand EditCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseProductStatusMessage(true, ProductId, ProductName, Article, Description, PricePerUnit, PricePerUnitKg, Mark, Coefficient)));

    private string _status = "new";
    [UsedImplicitly]
    public string Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    private double? _createdBy;
    public double? CreatedBy
    {
        get => _createdBy;
        set => this.RaiseAndSetIfChanged(ref _createdBy, value);
    }

    private DateTime? _createdAt;
    public DateTime? CreatedAt
    {
        get => _createdAt;
        set => this.RaiseAndSetIfChanged(ref _createdAt, value);
    }

    public string StatusDisplay => Status switch
    {
        "new" => "Новая",
        "assigned" => "Разбита на подмарки",
        "in_progress" => "В работе",
        "completed" => "Завершена",
        _ => Status
    };

    [UsedImplicitly]
    public double Id { get; set; }
    [UsedImplicitly]
    public double ProductId { get; set; }

    [UsedImplicitly]
    public string Notes { get; set; } = string.Empty;
    [UsedImplicitly]
    public string ProductName { get; set; } = string.Empty;
    [UsedImplicitly]
    public string Mark { get; set; } = string.Empty;
    [UsedImplicitly]
    public string Article { get; set; } = string.Empty;
    [UsedImplicitly]
    public string Description { get; set; } = string.Empty;

    [UsedImplicitly]
    public decimal PricePerUnit { get; set; }

    [UsedImplicitly]
    public decimal PricePerUnitKg { get; set; }
    [UsedImplicitly]
    public decimal Coefficient { get; set; }

    private bool _canCompleteTask;
    [UsedImplicitly]
    public bool CanCompleteTask 
    {
        get => _canCompleteTask;
        set
        {
            this.RaiseAndSetIfChanged(ref _canCompleteTask, value);
            this.RaisePropertyChanged(nameof(IsAdministratorOrMasterAndCanCompleteTask));
        }
    }

    public ICommand DeleteCommand
        => new RelayCommand(() =>
        {
            string[] deleteQueries =
            [
                "DELETE FROM public.shipments WHERE product_id = @id",
                    "DELETE FROM public.production WHERE product_id = @id"
            ];

            var viewModel = new ConfirmDeleteWindowViewModel(ProductId, ProductName, "DELETE FROM public.product WHERE id = @id", () => WeakReferenceMessenger.Default.Send(new RefreshProductListMessage()), deleteQueries);

            var window = new ConfirmDeleteWindow()
            {
                DataContext = viewModel,
            };

            viewModel.SetWindow(window);

            window.Show();
        });

    [UsedImplicitly]
    public static bool IsAdministratorOrMasterAndManager
        => ManagerCookie.IsUserLoggedIn()
        && (ManagerCookie.IsAdministrator || ManagerCookie.IsMaster || ManagerCookie.IsManager);

    public bool IsAdministratorOrMasterAndCanCompleteTask
        => IsAdministratorOrMasterAndManager 
        && CanCompleteTask
        && Status != "completed";

    public async Task CheckIsTaskCanBeCompleted() 
    {
        try
        {
            const string sql = @"
                        SELECT COUNT(*) = 0 
                        FROM public.sub_product_operations spo
                        JOIN public.sub_products sp ON sp.id = spo.sub_product_id
                        WHERE sp.product_task_id = @task_id
                            AND spo.completed_quantity < spo.planned_quantity";
            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@task_id", Id);

            var result = await command.ExecuteScalarAsync();
            CanCompleteTask = result != null && Convert.ToBoolean(result);
        }
        catch (PostgresException ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);

            CanCompleteTask = false;
        }
        catch (Exception ex) 
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);

            CanCompleteTask = false;
        }
    }

    private async Task CompleteTaskAndShipAsync() 
    {
        try
        {
            const string getDateSql = "SELECT pt.product_id, SUM(spo.planned_quantity) " +
                "FROM public.product_tasks pt " +
                "JOIN public.sub_products sp ON sp.product_task_id = pt.id " +
                "JOIN public.sub_product_operations spo ON spo.sub_product_id = sp.id " +
                "WHERE pt.id = @task_id " +
                "GROUP BY pt.product_id";
            const string toShipmentSql = "INSERT INTO public.shipments (product_task_id, product_id, planned_quantity, shipped_quantity, created_by, status, shipment_date) " +
                "VALUES (@task_id, @product_id, @qty, @qty, @user_id, 'ready', CURRENT_DATE)";
            const string closeTaskSql = "UPDATE public.product_tasks SET status = 'completed' WHERE id = @id";
            const string productActiveSql = "UPDATE public.product SET is_active = false WHERE id = @pId";

            decimal totalQuantity = 0;

            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();

            await using (var command1 = new NpgsqlCommand(getDateSql, connection))
            {
                command1.Parameters.AddWithValue("@task_id", Id);
                await using var reader = await command1.ExecuteReaderAsync();
                if (await reader.ReadAsync())
                {
                    totalQuantity = reader.GetDecimal(1);
                }

                await reader.CloseAsync();
            }

            await using (var command2 = new NpgsqlCommand(toShipmentSql, connection))
            {
                command2.Parameters.AddWithValue("@task_id", Id);
                command2.Parameters.AddWithValue("@product_id", ProductId);
                command2.Parameters.AddWithValue("@qty", totalQuantity);
                command2.Parameters.AddWithValue("@user_id", ManagerCookie.GetIdUser ?? 0);
                await command2.ExecuteNonQueryAsync();
            }

            await using var command3 = new NpgsqlCommand(closeTaskSql, connection);
            command3.Parameters.AddWithValue("@id", Id);
            await command3.ExecuteNonQueryAsync();

            await using var command4 = new NpgsqlCommand(productActiveSql, connection);
            command4.Parameters.AddWithValue("@pId", ProductId);
            await command4.ExecuteNonQueryAsync();
                
            WeakReferenceMessenger.Default.Send(new RefreshProductListMessage());
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
