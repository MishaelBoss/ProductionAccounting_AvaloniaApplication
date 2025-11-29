using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using static ProductionAccounting_AvaloniaApplication.ViewModels.Control.NotFoundUserControlViewModel;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class ProductViewUserControlViewModel : ViewModelBase, INotifyPropertyChanged, IRecipient<RefreshSubProductListMessage>
{
    public ProductViewUserControlViewModel(double productId) 
    {
        ProductId = productId;

        _ = LoadTasksAsync();

        WeakReferenceMessenger.Default.Register<RefreshSubProductListMessage>(this);
    }

    public void Receive(RefreshSubProductListMessage message)
    {
        _ = LoadSubProductAsync();
    }

    public StackPanel? SubProductContent { get; set; } = null;

    private List<CartSubProductUserControl> subProductList = [];

    public ICommand AddSubProductCommand
        => new RelayCommand(() => { WeakReferenceMessenger.Default.Send(new OpenOrCloseTaskDateilStatusMessage(true, ProductId)); });

    public double ProductId { get; set; }

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

    public async Task LoadTasksAsync()
    {
        try
        {
            string sql = @"
                    SELECT 
                        p.name,
                        COALESCE(p.mark, '') AS mark,
                        p.article,
                        p.unit,
                        p.price_per_unit,
                        p.coefficient,
                        COALESCE(pt.status, 'new') AS status,
                        pt.created_at
                    FROM public.product_tasks pt
                    JOIN public.product p ON p.id = pt.product_id
                    WHERE COALESCE(pt.status, 'new') IN ('new', 'in_progress')
                      AND p.is_active = true AND pt.id = @product_task_id
                    ORDER BY pt.created_at DESC";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@product_task_id", ProductId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync()) 
                        {
                            Name = Name = reader.GetString(0);
                            Mark = reader.GetString(1);
                            Article = reader.GetString(2);
                            Unit = reader.GetString(3);
                            PricePerUnit = reader.GetInt32(4);
                            Coefficient = reader.GetInt32(5);
                            Status = reader.GetString(6);
                            CreatedAt = reader.GetDateTime(7);
                        }
                    }
                }
            }
        }
        catch (NpgsqlException ex)
        {
            Loges.LoggingProcess(LogLevel.CRITICAL, "Error connect to DB", 
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.WARNING,
                ex: ex);
        }
    }

    public async Task LoadSubProductAsync()
    {
        StackPanelHelper.ClearAndRefreshStackPanel<CartSubProductUserControl>(SubProductContent, subProductList);

        try
        {
            string sql = "SELECT id, product_task_id, name, planned_quantity, planned_weight, notes, created_at FROM public.sub_products WHERE product_task_id = @product_task_id";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@product_task_id", ProductId);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var viewModel = new CartSubProductUserControlViewModel()
                            {
                                Id = reader.GetDouble(0),
                                ProductTaskId = reader.GetDouble(1),
                                Name = reader.GetString(2),
                                PlannedQuantity = reader.GetDouble(3),
                                PlannedWeight = reader.GetDouble(4),
                                Notes = reader.GetString(5),
                            };

                            var userControl = new CartSubProductUserControl()
                            {
                                DataContext = viewModel,
                            };

                            subProductList.Add(userControl);
                        }
                    }
                }
            }

            StackPanelHelper.RefreshStackPanelContent<CartSubProductUserControl>(SubProductContent, subProductList);

            if (subProductList.Count == 0) ItemNotFoundException.Show(SubProductContent, ErrorLevel.NotFound);
        }
        catch (Exception ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartSubProductUserControl>(SubProductContent, subProductList);
            ItemNotFoundException.Show(SubProductContent, ErrorLevel.NoConnectToDB);
            Loges.LoggingProcess(LogLevel.ERROR, ex: ex);
        }
    }
}
