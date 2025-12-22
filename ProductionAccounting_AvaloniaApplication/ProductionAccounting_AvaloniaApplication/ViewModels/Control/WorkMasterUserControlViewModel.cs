using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static ProductionAccounting_AvaloniaApplication.ViewModels.Control.NotFoundUserControlViewModel;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class WorkMasterUserControlViewModel : ViewModelBase, IRecipient<RefreshProductListMessage>
{
    public WorkMasterUserControlViewModel() 
    {
        WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(RefreshProductListMessage message)
    {
        _ = LoadTasksAsync();
    }

    public StackPanel? CartTasks { get; set; }

    private readonly List<CartOrderUserControl> _productList = [];

    public async Task LoadTasksAsync()
    {
        if (!ManagerCookie.IsUserLoggedIn() && (!ManagerCookie.IsMaster || !ManagerCookie.IsAdministrator)) return;

        StackPanelHelper.ClearAndRefreshStackPanel(CartTasks, _productList);

        try
        {
            string sql = @"
                    SELECT 
                        pt.id,
                        pt.product_id,
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
                        AND p.is_active = true
                        AND pt.assigned_by = @UserId
                    ORDER BY pt.created_at DESC";

            await using (var connection = new NpgsqlConnection(Arguments.Connection))
            {
                await connection.OpenAsync();
                await using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@UserId", ManagerCookie.GetIdUser ?? 0);
                await using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var viewModel = new CartOrderUserControlViewModel
                    {
                        OrderId = reader.GetDouble(0),
                        TaskProductId = reader.GetDouble(1),
                        Name = reader.GetString(2),
                        Counterparties = reader.GetString(3),
                        TotalWeight = reader.GetDecimal(4),
                        Coefficient = reader.GetDecimal(5),
                        Status = reader.GetString(6),
                        CreatedAt = reader.GetDateTime(7),
                        Description = reader.GetString(8),
                    };

                    var userControl = new CartOrderUserControl()
                    {
                        DataContext = viewModel,
                    };

                    _productList.Add(userControl);
                }

                if (_productList.FirstOrDefault()?.DataContext is CartOrderUserControlViewModel vm) await vm.CheckIsTaskCanBeCompleted();
            }

            StackPanelHelper.RefreshStackPanelContent(CartTasks, _productList);

            if (_productList.Count == 0) ItemNotFoundException.Show(CartTasks, ErrorLevel.NotFound);
        }
        catch (NpgsqlException ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel(CartTasks, _productList);
            ItemNotFoundException.Show(CartTasks, ErrorLevel.NoConnectToDb);
            Loges.LoggingProcess(LogLevel.Critical, "Error connect to DB", ex: ex);
        }
        catch (Exception ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel(CartTasks, _productList);
            Loges.LoggingProcess(LogLevel.Warning, 
                ex: ex);
        }
    }
}
