using Avalonia.Controls;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class WorkUserControlViewModel : ViewModelBase, INotifyPropertyChanging
{
    public WorkUserControlViewModel() 
    {
    }

    public StackPanel? CartTasks { get; set; } = null;

    private List<CartTaskUserControl> tasksList = [];

    public async Task LoadTodayAsync()
    {
        StackPanelHelper.ClearAndRefreshStackPanel<CartTaskUserControl>(CartTasks, tasksList);

        try
        {
            var sql = @"
                SELECT 
                    p.id,
                    p.quantity, 
                    p.amount, 
                    p.notes, 
                    p.created_at, 
                    p.status,
                    pr.name AS product_name, pr.unit,
                    COALESCE(o.name, '—') AS operation_name
                FROM public.production p
                JOIN public.product pr ON pr.id = p.product_id
                LEFT JOIN public.operation o ON o.id = p.operation_id
                WHERE p.user_id = @user_id 
                    AND p.production_date = CURRENT_DATE
                ORDER BY p.created_at DESC";

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
                            var viewModel = new CartTaskUserControlViewModel()
                            {
                                IsWork = true,
                                ProductionId = reader.GetDouble(0),
                                Quantity = reader.GetDecimal(1),
                                Amount = reader.GetDecimal(2),
                                Notes = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                CreatedAt = reader.GetDateTime(4),
                                Status = reader.GetString(5),
                                ProductName = reader.GetString(6),
                                Unit = reader.GetString(7),
                                OperationName = reader.GetString(8)
                            };

                            var cartUser = new CartTaskUserControl()
                            {
                                DataContext = viewModel
                            };

                            tasksList.Add(cartUser);
                        }
                    }
                }
            }

            StackPanelHelper.RefreshStackPanelContent<CartTaskUserControl>(CartTasks, tasksList);
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