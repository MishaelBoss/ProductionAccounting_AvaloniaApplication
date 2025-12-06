using Avalonia.Controls;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Threading.Tasks;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class WorkUserControlViewModel : ViewModelBase, INotifyPropertyChanging
{
    public StackPanel? CartTasks { get; set; } = null;

    private List<CartEmployeeTaskUserControl> tasksList = [];

    public async Task LoadTodayAsync()
    {
        StackPanelHelper.ClearAndRefreshStackPanel<CartEmployeeTaskUserControl>(CartTasks, tasksList);

        try
        {
            string sql = @"
                        SELECT 
                            sp.name AS sub_product_name,
                            o.name AS operation_name,
                            ta.assigned_quantity,
                            ta.notes,
                            ta.status,
                            ta.id AS assignment_id
                        FROM public.task_assignments ta
                        JOIN public.sub_product_operations spo ON spo.id = ta.sub_product_operation_id
                        JOIN public.sub_products sp ON sp.id = spo.sub_product_id
                        JOIN public.operation o ON o.id = spo.operation_id
                        WHERE ta.user_id = @user_id
                          AND ta.status = 'assigned'
                        ORDER BY ta.assigned_at DESC";

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
                                AssignmentId = reader.GetDouble("assignment_id"),
                                SubProductName = reader.GetString("sub_product_name"),
                                OperationName = reader.GetString("operation_name"),
                                AssignedQuantity = reader.GetDecimal("assigned_quantity"),
                                Notes = reader.IsDBNull("notes") ? null : reader.GetString("notes"),
                                Status = reader.GetString("status")
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