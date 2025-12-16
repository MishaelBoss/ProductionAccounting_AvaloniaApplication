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

public class ProductViewUserControlViewModel : ViewModelBase, INotifyPropertyChanged, IRecipient<RefreshSubProductListMessage>, IRecipient<OpenOrCloseSubProductStatusMessage>, IRecipient<RefreshSubProductOperationsMessage>
{
    public ProductViewUserControlViewModel(string name, double productId, string mark, decimal coefficient, string notes, string status) 
    {
        Name = name;
        ProductId = productId;
        Mark = mark;
        Coefficient = coefficient;
        Notes = notes;
        Status = status;

        WeakReferenceMessenger.Default.RegisterAll(this);
    }

    public void Receive(RefreshSubProductListMessage message)
    {
        _ = LoadSubProductAsync();
    }

    public void Receive(OpenOrCloseSubProductStatusMessage message)
    {
        LoadSubProductViewAsync(message.SubProductId);
    }

    public void Receive(RefreshSubProductOperationsMessage message)
    {
        _ = LoadDateSubOperationAsync(message.SubProductId);
    }

    public StackPanel? SubProductContent { get; set; } = null;
    public StackPanel? SubProductOperation { get; set; } = null;

    private List<CartSubProductUserControl> subProductList = [];
    private List<CartSubProductOperationUserControl> subProductOperationList = [];

    public ICommand AddSubProductCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseAddSubProductStatusMessage(true, ProductId)));

    public ICommand OpenEmployeeAssignmentMasterSubMarkUserControlViewModelCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseEmployeeAssignmentMasterSubMarkStatusMessage(true, ProductId)));

    public ICommand AddOperationCommand 
        => new RelayCommand(() => { if (SubProductId.HasValue) WeakReferenceMessenger.Default.Send(new OpenOrCloseSubOperationStatusMessage(true, SubProductId.Value)); });

    public string Name { get; }
    public double ProductId { get; }
    public string Mark { get; }
    public decimal Coefficient { get; }
    public string Notes { get; }
    public string Status { get; set; }

    private double? _subProductId = 0;
    public double? SubProductId
    {
        get => _subProductId;
        set => this.RaiseAndSetIfChanged(ref _subProductId, value);
    }

    private string? _subProductName = string.Empty;
    public string? SubProductName
    {
        get => _subProductName;
        set => this.RaiseAndSetIfChanged(ref _subProductName, value);
    }

    private decimal? _subProductPlannedQuantity = 0;
    public decimal? SubProductPlannedQuantity
    {
        get => _subProductPlannedQuantity;
        set => this.RaiseAndSetIfChanged(ref _subProductPlannedQuantity, value);
    }

    private decimal? _subProductPlannedWeight = 0;
    public decimal? SubProductPlannedWeight
    {
        get => _subProductPlannedWeight;
        set => this.RaiseAndSetIfChanged(ref _subProductPlannedWeight, value);
    }

    private string? _subProductNotes = string.Empty;
    public string? SubProductNotes
    {
        get => _subProductNotes;
        set => this.RaiseAndSetIfChanged(ref _subProductNotes, value);
    }

    public bool IsAdministratorOrMasterAndManager
        => ManagerCookie.IsUserLoggedIn()
        && (ManagerCookie.IsAdministrator || ManagerCookie.IsMaster || ManagerCookie.IsManager);

    public bool IsAdministratorOrMasterAndCanCompleteTask
        => IsAdministratorOrMasterAndManager 
        && Status != "completed";
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
        catch (NpgsqlException ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartSubProductUserControl>(SubProductContent, subProductList);
            ItemNotFoundException.Show(SubProductContent, ErrorLevel.NoConnectToDB);

            Loges.LoggingProcess(LogLevel.ERROR,
                "Connection or request error",
                ex: ex);
        }
        catch (Exception ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartSubProductUserControl>(SubProductContent, subProductList);
            ItemNotFoundException.Show(SubProductContent, ErrorLevel.NoConnectToDB);
            Loges.LoggingProcess(LogLevel.ERROR, ex: ex);
        }
    }

    public async void LoadSubProductViewAsync(double subProductId)
    {
        await LoadDateSubProductAsync(subProductId);
        await LoadDateSubOperationAsync(subProductId);
    }

    private async Task LoadDateSubProductAsync(double subProductId)
    {
        try
        {
            string sql = @"SELECT id, name, planned_quantity, planned_weight, notes, created_at FROM public.sub_products WHERE id = @id";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                using (var command1 = new NpgsqlCommand(sql, connection))
                {
                    command1.Parameters.AddWithValue("@id", subProductId);

                    using (var reader1 = await command1.ExecuteReaderAsync())
                    {
                        if (await reader1.ReadAsync())
                        {
                            SubProductId = reader1.IsDBNull(0) ? 0 : reader1.GetDouble(0);
                            SubProductName = reader1.IsDBNull(1) ? string.Empty : reader1.GetString(1);
                            SubProductPlannedQuantity = reader1.IsDBNull(2) ? 0 : reader1.GetDecimal(2);
                            SubProductPlannedWeight = reader1.IsDBNull(3) ? 0 : reader1.GetDecimal(3);
                            SubProductNotes = reader1.IsDBNull(4) ? string.Empty : reader1.GetString(4);
                        }
                    }
                }
            }
        }
        catch (NpgsqlException ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartSubProductOperationUserControl>(SubProductOperation, subProductOperationList);
            ItemNotFoundException.Show(SubProductOperation, ErrorLevel.NoConnectToDB);

            Loges.LoggingProcess(LogLevel.ERROR,
                "Connection or request error",
                ex: ex);
        }
        catch (Exception ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartSubProductOperationUserControl>(SubProductOperation, subProductOperationList);
            ItemNotFoundException.Show(SubProductOperation, ErrorLevel.NoConnectToDB);
            Loges.LoggingProcess(LogLevel.ERROR, 
                ex: ex);
        }
    }

    private async Task LoadDateSubOperationAsync(double subProductId)
    {
        subProductOperationList.Clear();
        StackPanelHelper.ClearAndRefreshStackPanel<CartSubProductOperationUserControl>(SubProductOperation, subProductOperationList);

        try
        {
            string sqlSubProductOperations = @"
                                            SELECT 
                                                spo.id,
                                                o.name,
                                                spo.planned_quantity,
                                                spo.completed_quantity,
                                                spo.assigned_to_user_id,
                                                pt.product_id,
                                                o.id,
                                                u.login,
                                                sp.id,
                                                COALESCE(pt.status, 'new') AS status,
                                                o.price,
                                                o.time_required,
                                                o.description,
                                                o.operation_code
                                            FROM public.sub_product_operations spo
                                            JOIN public.operation o ON o.id = spo.operation_id
                                            JOIN public.sub_products sp ON sp.id = spo.sub_product_id
                                            JOIN public.product_tasks pt ON pt.id = sp.product_task_id
                                            LEFT JOIN public.user u ON u.id = spo.assigned_to_user_id
                                            WHERE spo.sub_product_id = @sub_product_id
                                            ORDER BY spo.id";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command2 = new NpgsqlCommand(sqlSubProductOperations, connection))
                {
                    command2.Parameters.AddWithValue("@sub_product_id", subProductId);
                    using (var reader2 = await command2.ExecuteReaderAsync())
                    {
                        while (await reader2.ReadAsync())
                        {
                            var viewModel = new CartSubProductOperationUserControlViewModel()
                            {
                                SubProductOperationId = reader2.GetDouble(0),
                                OperationName = reader2.GetString(1),
                                PlannedQuantity = reader2.GetDecimal(2),
                                CompletedQuantity = reader2.GetDecimal(3),
                                AssignedToUserId = reader2.IsDBNull(4) ? null : reader2.GetDouble(4),
                                ProductId = reader2.GetDouble(5),
                                OperationId = reader2.GetDouble(6),
                                UserName = reader2.IsDBNull(7) ? string.Empty : reader2.GetString(7),
                                SubProductId = reader2.GetDouble(8),
                                Status = reader2.GetString(9),
                                OperationPrice = reader2.GetDecimal(10),
                                OperationTime = reader2.GetDecimal(11),
                                OperationDescription = reader2.GetString(12),
                                OperationCode = reader2.GetString(13),
                            };

                            var userControl = new CartSubProductOperationUserControl()
                            {
                                DataContext = viewModel,
                            };

                            subProductOperationList.Add(userControl);
                        }
                    }
                }

                StackPanelHelper.RefreshStackPanelContent<CartSubProductOperationUserControl>(SubProductOperation, subProductOperationList);

                if(subProductOperationList.Count == 0) ItemNotFoundException.Show(SubProductOperation, ErrorLevel.NotFound);
            }
        }
        catch (NpgsqlException ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartSubProductOperationUserControl>(SubProductOperation, subProductOperationList);
            ItemNotFoundException.Show(SubProductOperation, ErrorLevel.NoConnectToDB);
            Loges.LoggingProcess(LogLevel.ERROR,
                "Connection or request error",
                ex: ex);
        }
        catch (Exception ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartSubProductOperationUserControl>(SubProductOperation, subProductOperationList);
            ItemNotFoundException.Show(SubProductOperation, ErrorLevel.NoConnectToDB);
            Loges.LoggingProcess(LogLevel.ERROR, 
                ex: ex);
        }
    }
}