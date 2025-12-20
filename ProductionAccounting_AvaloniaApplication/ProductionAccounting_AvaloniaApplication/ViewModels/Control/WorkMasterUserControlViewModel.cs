using Avalonia.Controls;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using static ProductionAccounting_AvaloniaApplication.ViewModels.Control.NotFoundUserControlViewModel;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class WorkMasterUserControlViewModel : ViewModelBase, INotifyPropertyChanging, IRecipient<RefreshProductListMessage>
{
    public WorkMasterUserControlViewModel() 
    {
        WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(RefreshProductListMessage message)
    {
        _ = LoadTasksAsync();
    }

    public StackPanel? CartTasks { get; set; } = null;

    private readonly List<CartProductUserControl> productList = [];

    public async Task LoadTasksAsync()
    {
        if (!ManagerCookie.IsUserLoggedIn() && (!ManagerCookie.IsMaster || !ManagerCookie.IsAdministrator)) return;

        StackPanelHelper.ClearAndRefreshStackPanel<CartProductUserControl>(CartTasks, productList);

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

            using (var connection = new NpgsqlConnection(Arguments.Connection))
            {
                await connection.OpenAsync();
                using var command = new NpgsqlCommand(sql, connection);
                command.Parameters.AddWithValue("@UserId", ManagerCookie.GetIdUser ?? 0);
                using var reader = await command.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var viewModel = new CartProductUserControlViewModel()
                    {
                        Id = reader.GetDouble(0),
                        ProductId = reader.GetDouble(1),
                        ProductName = reader.GetString(2),
                        Mark = reader.GetString(3),
                        Article = reader.GetString(4),
                        Unit = reader.GetString(5),
                        PricePerUnit = reader.GetDecimal(6),
                        Coefficient = reader.GetDecimal(7),
                        Status = reader.GetString(8),
                        CreatedAt = reader.GetDateTime(9)
                    };

                    var userControl = new CartProductUserControl()
                    {
                        DataContext = viewModel,
                    };

                    productList.Add(userControl);
                }

                if (productList.FirstOrDefault()?.DataContext is CartProductUserControlViewModel vm) await vm.CheckIsTaskCanBeCompleted();
            }

            StackPanelHelper.RefreshStackPanelContent<CartProductUserControl>(CartTasks, productList);

            if (productList.Count == 0) ItemNotFoundException.Show(CartTasks, ErrorLevel.NotFound);
        }
        catch (NpgsqlException ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartProductUserControl>(CartTasks, productList);
            ItemNotFoundException.Show(CartTasks, ErrorLevel.NoConnectToDB);
            Loges.LoggingProcess(LogLevel.CRITICAL, "Error connect to DB", ex: ex);
        }
        catch (Exception ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartProductUserControl>(CartTasks, productList);
            Loges.LoggingProcess(LogLevel.WARNING, 
                ex: ex);
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
