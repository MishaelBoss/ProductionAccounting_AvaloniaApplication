using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using static ProductionAccounting_AvaloniaApplication.ViewModels.Control.NotFoundUserControlViewModel;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class TabProductUserControlViewModel : ViewModelBase, INotifyPropertyChanged, IRecipient<RefreshProductListMessage>
{
    public TabProductUserControlViewModel()
    {
        WeakReferenceMessenger.Default.Register<RefreshProductListMessage>(this);
    }

    public void Receive(RefreshProductListMessage message)
        => GetList();

    public StackPanel? HomeMainContent { get; set; } = null;

    private List<CartProductUserControl> productList = [];
    private List<double> filteredProductIds = [];

    public ICommand ResetFiltersCommand
        => new RelayCommand(() => ResetFilters());

    public ICommand DownloadAsyncCommand
        => new RelayCommand(async () => await DownloadListAsync());

    public ICommand RefreshAsyncCommand
        => new RelayCommand(() => GetList());

    private string _search = string.Empty;
    public string Search
    {
        get => _search;
        set
        {
            if (_search != value)
            {
                _search = value;
                OnPropertyChanged(nameof(Search));
                PerformSearchList();
            }
        }
    }

    private bool _showActive = true;
    public bool ShowActive
    {
        get => _showActive;
        set
        {
            if (_showActive != value)
            {
                _showActive = value;
                OnPropertyChanged(nameof(ShowActive));
                _ = ApplyFilters();
            }
        }
    }

    private bool _showInactive = true;
    public bool ShowInactive
    {
        get => _showInactive;
        set
        {
            if (_showInactive != value)
            {
                _showInactive = value;
                OnPropertyChanged(nameof(ShowInactive));
                _ = ApplyFilters();
            }
        }
    }

    private async void PerformSearchList()
    {
        await ApplyFilters();
    }

    public async void GetList()
    {
        Search = string.Empty;
        await ApplyFilters();
    }

    private async Task ApplyFilters()
    {
        try
        {
            filteredProductIds = await GetFilteredProductIdsAsync();

            if (filteredProductIds.Count > 0)
            {
                await SearchProductAsync(filteredProductIds);
            }
            else
            {
                StackPanelHelper.ClearAndRefreshStackPanel<CartProductUserControl>(HomeMainContent, productList);
                ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NotFound);
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.ERROR,
                message: "Error applying filters",
                ex: ex);

            StackPanelHelper.ClearAndRefreshStackPanel<CartProductUserControl>(HomeMainContent, productList);
        }
    }

    private async Task<List<double>> GetFilteredProductIdsAsync()
    {
        var userIds = new List<double>();

        try
        {
            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                var sql = "SELECT DISTINCT id FROM public.product WHERE 1=1";

                var parameters = new List<NpgsqlParameter>();

                if (!ShowActive || !ShowInactive)
                {
                    if (ShowActive && !ShowInactive)
                    {
                        sql += " AND is_active = true";
                    }
                    else if (!ShowActive && ShowInactive)
                    {
                        sql += " AND is_active = false";
                    }
                }

                if (!string.IsNullOrWhiteSpace(Search) && Search != "%")
                {
                    sql += " AND name ILIKE @search";
                    parameters.Add(new NpgsqlParameter("@search", $"%{Search}%"));
                }

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(param);
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            if (!reader.IsDBNull(0))
                            {
                                userIds.Add(reader.GetDouble(0));
                            }
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.ERROR, "Error getting filtered user IDs", ex: ex);
        }

        return userIds;
    }

    private async Task SearchProductAsync(List<double> userIds)
    {
        //if (!ManagerCookie.IsUserLoggedIn() && !ManagerCookie.IsAdministrator) return;

        if (userIds.Count == 0)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartProductUserControl>(HomeMainContent, productList);
            ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NotFound);
            return;
        }

        StackPanelHelper.ClearAndRefreshStackPanel<CartProductUserControl>(HomeMainContent, productList);

        try
        {
            var parameters = new List<NpgsqlParameter>();
            var paramNames = new List<string>();

            for (int i = 0; i < userIds.Count; i++)
            {
                var paramName = $"@id{i}";
                paramNames.Add(paramName);
                parameters.Add(new NpgsqlParameter(paramName, userIds[i]));
            }

            string sql = $@"
                    SELECT 
                        pt.id,
                        pt.product_id,
                        p.name,
                        COALESCE(p.mark, '') AS mark,
                        p.article,
                        p.unit,
                        p.price_per_unit,
                        p.price_per_kg,
                        p.coefficient,
                        COALESCE(pt.status, 'new') AS status,
                        pt.created_at,
                        p.description
                    FROM public.product_tasks pt
                    JOIN public.product p ON p.id = pt.product_id
                    WHERE p.id IN ({string.Join(", ", paramNames)})
                    ORDER BY pt.created_at DESC";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
                {

                    foreach (var param in parameters)
                    {
                        command.Parameters.Add(param);
                    }

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            var viewModel = new CartProductUserControlViewModel
                            {
                                Id = reader.GetDouble(0),
                                ProductId = reader.GetDouble(1),
                                ProductName = reader.GetString(2),
                                Mark = reader.GetString(3),
                                Article = reader.GetString(4),
                                Unit = reader.GetString(5),
                                PricePerUnit = reader.GetDecimal(6),
                                PricePerUnitKg = reader.GetDecimal(7),
                                Coefficient = reader.GetDecimal(8),
                                Status = reader.GetString(9),
                                CreatedAt = reader.GetDateTime(10),
                                Description = reader.GetString(11),
                            };

                            var userControl = new CartProductUserControl
                            {
                                DataContext = viewModel
                            };

                            productList.Add(userControl);
                        }
                    }
                }
            }

            StackPanelHelper.RefreshStackPanelContent<CartProductUserControl>(HomeMainContent, productList);

            if (productList.Count == 0) ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NotFound);
        }
        catch (NpgsqlException ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartProductUserControl>(HomeMainContent, productList);
            ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NoConnectToDB);
            Loges.LoggingProcess(LogLevel.CRITICAL,
                "Connection or request error",
                ex: ex);
        }
        catch (Exception ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartProductUserControl>(HomeMainContent, productList);
            Loges.LoggingProcess(LogLevel.ERROR,
                "Error loading users by IDs",
                ex: ex);
        }
    }

    public async Task DownloadListAsync()
    {
        if (!ManagerCookie.IsUserLoggedIn()) return;

        try
        {
            string sql = @"
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

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var destinationPath = Paths.DestinationPathDB("AdminPanel", "Products");
                        Directory.CreateDirectory(destinationPath);

                        var fileSave = Path.Combine(destinationPath, Files.DBEquipments);

                        using (var writer = new StreamWriter(fileSave))
                        {
                            for (int i = 0; i < reader.FieldCount; i++)
                            {
                                writer.Write(reader.GetName(i));
                                if (i < reader.FieldCount - 1)
                                {
                                    writer.Write(",");
                                }
                            }
                            await writer.WriteLineAsync();

                            while (await reader.ReadAsync())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    var value = reader.GetValue(i);
                                    var stringValue = value?.ToString() ?? "";

                                    if (stringValue.Contains(",") || stringValue.Contains("\"") || stringValue.Contains("\n"))
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
                }
            }
        }
        catch (NpgsqlException ex)
        {
            Loges.LoggingProcess(LogLevel.ERROR, 
                "Database error downloading product list", 
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.ERROR, 
                "Error downloading product list", 
                ex: ex);
        }
    }

    private void ResetFilters()
    {
        ShowActive = true;
        ShowInactive = true;
        Search = string.Empty;
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
