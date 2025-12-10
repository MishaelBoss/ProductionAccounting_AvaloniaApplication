using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using Tmds.DBus.Protocol;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartProductUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
    public ICommand DeleteCommand
        => new RelayCommand(() =>
        {
            string[] deleteQueries =
            {
                "DELETE FROM public.production WHERE product_id = @id"
            };

            var viewModel = new ConfirmDeleteWindowViewModel(ProductId, Name, "DELETE FROM public.product WHERE id = @id", (() => WeakReferenceMessenger.Default.Send(new RefreshProductListMessage())), deleteQueries);

            var window = new ConfirmDeleteWindow()
            {
                DataContext = viewModel,
            };

            viewModel.SetWindow(window);

            window.Show();
        });

    public ICommand OpenPruductViewCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseProductViewStatusMessage(true, Name, Id, Mark, Coefficient, Notes)));

    public ICommand CompleteTaskCommand
        => new RelayCommand(async () => { if (!CanCompleteTask) return; await CompleteTaskAndShipAsync(); });

    private string _status = "new";
    public string Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    private string? _notes;
    public string? Notes
    {
        get => _notes;
        set => this.RaiseAndSetIfChanged(ref _notes, value);
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

    private string? _productName;
    public string? ProductName
    {
        get => _productName;
        set => this.RaiseAndSetIfChanged(ref _productName, value);
    }

    private string _mark = string.Empty;
    public string Mark
    {
        get => _mark;
        set => this.RaiseAndSetIfChanged(ref _mark, value);
    }

    public string StatusDisplay => Status switch
    {
        "new" => "Новая",
        "assigned" => "Разбита на подмарки",
        "in_progress" => "В работе",
        "completed" => "Завершена",
        _ => Status
    };

    private double _id = 0;
    public double Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }

    private double _productId = 0;
    public double ProductId
    {
        get => _productId;
        set => this.RaiseAndSetIfChanged(ref _productId, value);
    }

    private string _name = string.Empty;
    public string Name 
    { 
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    private string _article = string.Empty;
    public string Article 
    {
        get => _article;
        set => this.RaiseAndSetIfChanged(ref _article, value);
    }

    private Int32 _pricePerUnit;
    public Int32 PricePerUnit 
    { 
        get => _pricePerUnit;
        set => this.RaiseAndSetIfChanged(ref _pricePerUnit, value);
    }

    private string _unit = string.Empty;
    public string Unit
    {
        get => _unit;
        set => this.RaiseAndSetIfChanged(ref _unit, value);
    }

    private Int32 _coefficient;
    public Int32 Coefficient
    {
        get => _coefficient;
        set => this.RaiseAndSetIfChanged(ref _coefficient, value);
    }

    private bool _canCompleteTask;
    public bool CanCompleteTask 
    {
        get => _canCompleteTask;
        set
        {
            if (_canCompleteTask != value) 
            {
                _canCompleteTask = value;
                OnPropertyChanged(nameof(CanCompleteTask));
                OnPropertyChanged(nameof(IsAdministratorOrMasterAndCanCompleteTask));
            }
        }
    }

    public bool IsAdministratorOrMaster
        => ManagerCookie.IsUserLoggedIn()
        && (ManagerCookie.IsMaster || ManagerCookie.IsAdministrator);

    public bool IsAdministratorOrMasterAndCanCompleteTask
        => IsAdministratorOrMaster && CanCompleteTask;

    public async Task CheckIsTaskCanBeCompleted() 
    {
        try
        {
            string sql = @"
                        SELECT COUNT(*) = 0 
                        FROM public.sub_product_operations spo
                        JOIN public.sub_products sp ON sp.id = spo.sub_product_id
                        WHERE sp.product_task_id = @task_id
                          AND spo.completed_quantity < spo.planned_quantity";
            using (var connection = new NpgsqlConnection(Arguments.connection)) 
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection)) 
                {
                    command.Parameters.AddWithValue("@task_id", Id);

                    CanCompleteTask = (bool)await command.ExecuteScalarAsync();
                }
            }
        }
        catch (PostgresException ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);

            CanCompleteTask = false;
        }
        catch (Exception ex) 
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);

            CanCompleteTask = false;
        }
    }

    private async Task CompleteTaskAndShipAsync() 
    {
        try
        {
            string getDateSql = "SELECT pt.product_id, SUM(spo.planned_quantity) " +
                "FROM public.product_tasks pt " +
                "JOIN public.sub_products sp ON sp.product_task_id = pt.id " +
                "JOIN public.sub_product_operations spo ON spo.sub_product_id = sp.id " +
                "WHERE pt.id = @task_id " +
                "GROUP BY pt.product_id";
            string toShipmentSql = "INSERT INTO public.shipments (product_task_id, product_id, planned_quantity, shipped_quantity, created_by, status, shipment_date) " +
                "VALUES (@task_id, @product_id, @qty, @qty, @user_id, 'ready', CURRENT_DATE)";
            string closeTaskSql = "UPDATE public.product_tasks SET status = 'completed' WHERE id = @id";

            double productId = 0;
            decimal totalQuantity = 0;

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                using (var command1 = new NpgsqlCommand(getDateSql, connection))
                {
                    command1.Parameters.AddWithValue("@task_id", Id);
                    using (var reader = await command1.ExecuteReaderAsync()) 
                    {
                        if (await reader.ReadAsync()) 
                        {
                            productId = reader.GetDouble(0);
                            totalQuantity = reader.GetDecimal(1);
                        }

                        await reader.CloseAsync();
                    }
                }

                using (var command2 = new NpgsqlCommand(toShipmentSql, connection))
                {
                    command2.Parameters.AddWithValue("@task_id", Id);
                    command2.Parameters.AddWithValue("@product_id", ProductId);
                    command2.Parameters.AddWithValue("@qty", totalQuantity);
                    command2.Parameters.AddWithValue("@user_id", ManagerCookie.GetIdUser ?? 0);
                    await command2.ExecuteNonQueryAsync();
                }

                using (var command3 = new NpgsqlCommand(closeTaskSql, connection))
                {
                    command3.Parameters.AddWithValue("@id", Id);
                    await command3.ExecuteNonQueryAsync();

                    WeakReferenceMessenger.Default.Send(new RefreshProductListMessage());
                }
            }
        }
        catch (PostgresException ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);
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
