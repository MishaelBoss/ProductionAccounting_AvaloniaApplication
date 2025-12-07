using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading.Tasks;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class WorkUserControlViewModel : ViewModelBase, INotifyPropertyChanging, IRecipient<RefreshEmployeeTasksMessage>
{
    public WorkUserControlViewModel() 
    {
        WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(RefreshEmployeeTasksMessage message)
    {
        _ = LoadTodayAsync();
    }

    public StackPanel? CartTasks { get; set; } = null;

    private List<CartEmployeeTaskUserControl> tasksList = [];

    public async Task LoadTodayAsync()
    {
        StackPanelHelper.ClearAndRefreshStackPanel<CartEmployeeTaskUserControl>(CartTasks, tasksList);

        try
        {
            string sql = @"
                        SELECT 
                            spo.id AS sub_product_operation_id,
                            sp.name AS sub_product_name,
                            o.name AS operation_name,
                            spo.planned_quantity,
                            spo.completed_quantity,
                            spo.notes,
                            pt.product_id,
                            o.id AS operation_id,
                            spo.assigned_to_user_id
                        FROM public.sub_product_operations spo
                        JOIN public.sub_products sp ON sp.id = spo.sub_product_id
                        JOIN public.product_tasks pt ON pt.id = sp.product_task_id
                        JOIN public.operation o ON o.id = spo.operation_id
                        WHERE spo.assigned_to_user_id = @user_id
                            AND spo.completed_quantity < spo.planned_quantity
                        ORDER BY spo.created_at DESC";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@user_id", ManagerCookie.GetIdUser ?? 0);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var viewModel = new CartEmployeeTaskUserControlViewModel()
                            {
                                SubProductOperationId = reader.GetDouble("sub_product_operation_id"),
                                SubProductName = reader.GetString("sub_product_name"),
                                OperationName = reader.GetString("operation_name"),
                                PlannedQuantity = reader.GetDecimal("planned_quantity"),
                                CompletedQuantity = reader.GetDecimal("completed_quantity"),
                                Notes = reader.IsDBNull("notes") ? null : reader.GetString("notes"),
                                ProductId = reader.GetDouble("product_id"),
                                OperationId = reader.GetDouble("operation_id")
                            };

                            var cartUser = new CartEmployeeTaskUserControl()
                            {
                                DataContext = viewModel
                            };

                            tasksList.Add(cartUser);
                        }
                    }
                }
            }

            StackPanelHelper.RefreshStackPanelContent<CartEmployeeTaskUserControl>(CartTasks, tasksList);
        }
        catch (NpgsqlException ex)
        {
            Loges.LoggingProcess(LogLevel.CRITICAL,
                "Connection or request error",
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.WARNING,
                "Error loading users by IDs",
                ex: ex);
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}