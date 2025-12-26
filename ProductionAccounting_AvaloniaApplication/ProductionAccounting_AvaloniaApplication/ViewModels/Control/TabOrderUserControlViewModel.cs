using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using ReactiveUI;
using System.Threading;
using static ProductionAccounting_AvaloniaApplication.ViewModels.Control.NotFoundUserControlViewModel;
using MsBox.Avalonia;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class TabOrderUserControlViewModel : ViewModelBase, IRecipient<RefreshProductListMessage>
{
    private string _search = string.Empty;

    [UsedImplicitly]
    public string Search
    {
        get => _search;
        set
        {
            this.RaiseAndSetIfChanged(ref _search, value);
            _ = ScheduleSearchAsync();
        }
    }

    private bool _showActive = true;

    [UsedImplicitly]
    public bool ShowActive
    {
        get => _showActive;
        set
        {
            this.RaiseAndSetIfChanged(ref _showActive, value);
            _ = ScheduleSearchAsync();
        }
    }

    private bool _showInactive = true;

    [UsedImplicitly]
    public bool ShowInactive
    {
        get => _showInactive;
        set
        {
            this.RaiseAndSetIfChanged(ref _showInactive, value);
            _ = ScheduleSearchAsync();
        }
    }
    
    private CancellationTokenSource _searchCancellationTokenSource = new();
    private readonly Lock _loadingLock = new();
    private bool _isLoading;

    public TabOrderUserControlViewModel()
    {
        WeakReferenceMessenger.Default.Register(this);
    }

    public async void Receive(RefreshProductListMessage message)
    {
        try
        {
            await LoadProductsWithResetAsync();
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Critical, 
                ex: ex);
        }
    }

    public StackPanel? HomeMainContent { get; set; }

    private readonly List<CartOrderUserControl> _orderList = [];

    public ICommand ResetFiltersCommand
        => new RelayCommand(ResetFilters);

    public static ICommand DownloadAsyncCommand
        => new RelayCommand(async void () =>
        {
            try
            {
                await DownloadListAsync();
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.Critical,
                    ex: ex, message:
                    "Error download");
            }
        });

    public ICommand RefreshAsyncCommand
        => new RelayCommand(async void () =>
        {
            try
            {
                await LoadProductsWithResetAsync();
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.Critical, 
                    ex: ex, message: "Error refresh");
            }
        });

    private async Task ScheduleSearchAsync()
    {
        try
        {
            await _searchCancellationTokenSource.CancelAsync();
            _searchCancellationTokenSource = new CancellationTokenSource();
            
            await Task.Delay(300, _searchCancellationTokenSource.Token);
            
            _ = LoadProductsWithResetAsync();
        }
        catch (OperationCanceledException)
        {
            Loges.LoggingProcess(level: LogLevel.Info, 
                message: "Search cancel");
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Error, 
                ex: ex, 
                message: "Error scheduling search");
        }
    }

    public async Task LoadProductsWithResetAsync()
    {
        lock (_loadingLock)
        {
            if (_isLoading) return;
            _isLoading = true;
        }

        try
        {
            ClearProductList();

            var filteredProductIds = await GetFilteredProductIdsAsync();

            if (filteredProductIds.Count > 0)  await LoadProductsByIdsAsync(filteredProductIds);
            else  ShowNotFoundMessage();
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Error,
                message: "Error loading products",
                ex: ex);
            ClearProductList();
        }
        finally
        {
            lock (_loadingLock)
            {
                _isLoading = false;
            }
        }
    }

    private void ClearProductList()
    {
        try
        {
            _orderList.Clear();

            HomeMainContent?.Children.Clear();
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                message: "Error clearing product list",
                ex: ex);
        }
    }

    private void ShowNotFoundMessage()
    {
        try
        {
            ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NotFound);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                message: "Error showing not found message",
                ex: ex);
        }
    }
    
    private async Task<List<double>> GetFilteredProductIdsAsync()
    {
        var productIds = new List<double>();

        try
        {
            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();

            var sql = "SELECT DISTINCT id FROM public.order WHERE 1=1";
            var parameters = new List<NpgsqlParameter>();
            
            if (!ShowActive || !ShowInactive)
            {
                switch (ShowActive)
                {
                    case true when !ShowInactive:
                        sql += " AND is_active = true";
                        break;
                    case false when ShowInactive:
                        sql += " AND is_active = false";
                        break;
                }
            }
            
            if (!string.IsNullOrWhiteSpace(Search) && Search != "%")
            {
                sql += " AND (name ILIKE @search OR article ILIKE @search OR mark ILIKE @search)";
                parameters.Add(new NpgsqlParameter("@search", $"%{Search}%"));
            }

            await using var command = new NpgsqlCommand(sql, connection);
            foreach (var param in parameters)
            {
                command.Parameters.Add(param);
            }

            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                if (!reader.IsDBNull(0))
                {
                    productIds.Add(reader.GetDouble(0));
                }
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.Error, "Error getting filtered product IDs", ex: ex);
        }

        return productIds;
    }
    
    private async Task LoadProductsByIdsAsync(List<double> productIds)
    {
        if (productIds.Count == 0)
        {
            ShowNotFoundMessage();
            return;
        }

        try
        {
            var parameters = new List<NpgsqlParameter>();
            var paramNames = new List<string>();

            for (var i = 0; i < productIds.Count; i++)
            {
                var paramName = $"@id{i}";
                paramNames.Add(paramName);
                parameters.Add(new NpgsqlParameter(paramName, productIds[i]));
            }

            var sql = $@"
                    SELECT 
                        pt.order_id,
                        pt.id,
                        o.name,
                        o.counterparties,
                        o.total_weight,
                        o.coefficient,
                        COALESCE(pt.status, 'new') AS status,
                        pt.created_at,
                        o.description
                    FROM public.product_tasks pt
                    JOIN public.order o ON o.id = pt.order_id
                    WHERE o.id IN ({string.Join(", ", paramNames)}) 
                    ORDER BY pt.created_at DESC";

            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();
            
            await using var command = new NpgsqlCommand(sql, connection);
            foreach (var param in parameters)
            {
                command.Parameters.Add(param);
            }

            await using var reader = await command.ExecuteReaderAsync();
            
            var newProductList = new List<CartOrderUserControl>();
            
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

                newProductList.Add(new CartOrderUserControl
                {
                    DataContext = viewModel
                });
            }

            _orderList.Clear();
            _orderList.AddRange(newProductList);

            for (global::System.Int32 i = 0; i < newProductList.Count; i++) 
                if (newProductList[i]?.DataContext is CartOrderUserControlViewModel firstViewModel)
                    await firstViewModel.CheckIsTaskCanBeCompleted();

            if (HomeMainContent != null)
            {
                HomeMainContent.Children.Clear();
                foreach (var product in _orderList) 
                    HomeMainContent.Children.Add(product);
            }

            if (_orderList.Count == 0) ShowNotFoundMessage();
        }
        catch (NpgsqlException ex)
        {
            ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NoConnectToDb);
            Loges.LoggingProcess(LogLevel.Critical,
                "Connection or request error",
                ex: ex);
        }
        catch (Exception ex)
        {
            ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NoConnectToDb);
            Loges.LoggingProcess(LogLevel.Error,
                "Error loading products by IDs",
                ex: ex);
        }
    }

    private void ResetFilters()
    {
        try
        {
            _showActive = true;
            _showInactive = true;
            _search = string.Empty;
            
            this.RaisePropertyChanged(nameof(ShowActive));
            this.RaisePropertyChanged(nameof(ShowInactive));
            this.RaisePropertyChanged(nameof(Search));
            
            _ = LoadProductsWithResetAsync();
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Error,
                message: "Error resetting filters",
                ex: ex);
        }
    }

    private static async Task DownloadListAsync()
    {
        if (!ManagerCookie.IsUserLoggedIn()) return;

        try
        {
            const string sql = @"
                        SELECT 
                            p.*,
                            COALESCE(pt.task_count, 0) as task_count,
                            COALESCE(pt.active_task_count, 0) as active_task_count
                        FROM public.product p
                        LEFT JOIN (
                            SELECT 
                                product_id,
                                COUNT(*) as task_count,
                                SUM(CASE WHEN COALESCE(status, 'new') IN ('new', 'in_progress') THEN 1 ELSE 0 END) as active_task_count
                            FROM public.product_tasks
                            GROUP BY product_id
                        ) pt ON p.id = pt.product_id
                        ORDER BY p.name";

            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();

            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();
            var destinationPath = Paths.DestinationPathDb("AdminPanel", "Products");
            Directory.CreateDirectory(destinationPath);

            var fileSave = Path.Combine(destinationPath, Files.DbEquipments);

            await using (var writer = new StreamWriter(fileSave))
            {
                for (var i = 0; i < reader.FieldCount; i++)
                {
                    await writer.WriteAsync(reader.GetName(i));
                    if (i < reader.FieldCount - 1)
                    {
                        await writer.WriteAsync(",");
                    }
                }
                await writer.WriteLineAsync();

                while (await reader.ReadAsync())
                {
                    for (var i = 0; i < reader.FieldCount; i++)
                    {
                        var value = reader.GetValue(i);
                        var stringValue = value.ToString() ?? "";

                        if (stringValue.Contains(',') || stringValue.Contains('\"') || stringValue.Contains('\n'))
                        {
                            stringValue = $"\"{stringValue.Replace("\"", "\"\"")}\"";
                        }

                        await writer.WriteAsync(stringValue);

                        if (i < reader.FieldCount - 1)
                        {
                            await writer.WriteAsync(",");
                        }
                    }
                    await writer.WriteLineAsync();
                }
            }

            if (Arguments.OpenAfterDownloading)
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = fileSave,
                    UseShellExecute = true
                });
            }
        }
        catch (NpgsqlException ex)
        {
            Loges.LoggingProcess(LogLevel.Error, 
                "Database error downloading product list", 
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.Error, 
                "Error downloading product list", 
                ex: ex);
        }
    }
    
    ~TabOrderUserControlViewModel()
    {
           WeakReferenceMessenger.Default.Unregister<RefreshProductListMessage>(this);
    }
}