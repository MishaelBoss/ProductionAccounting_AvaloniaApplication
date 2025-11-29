using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using static ProductionAccounting_AvaloniaApplication.ViewModels.Control.NotFoundUserControlViewModel;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class TabOperationUserControlViewModel : ViewModelBase, INotifyPropertyChanged, IRecipient<RefreshOperationListMessage>
{
    public TabOperationUserControlViewModel()
    {
        WeakReferenceMessenger.Default.Register<RefreshOperationListMessage>(this);
    }

    public void Receive(RefreshOperationListMessage message)
    {
        GetList();
    }

    public StackPanel? HomeMainContent { get; set; } = null;

    private List<CartOperationUserControl> operationList = [];
    private List<double> filteredOperationIds = [];

    public ICommand ResetFiltersCommand
        => new RelayCommand(() => ResetFilters());

    public ICommand DownloadAsyncCommand
        => new RelayCommand(async () => await DownloadListAsync());

    public ICommand RefreshAsyncCommand
        => new RelayCommand(() => GetList());

    /*
        private bool _isOperationView = false;
        public bool IsOperationView
        {
            get => _isOperationView;
            set
            {
                if (_isOperationView != value)
                {
                    _isOperationView = value;
                    OnPropertyChanged(nameof(IsOperationView));
                }
            }
        }*/

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

    private void ResetFilters()
    {
        ShowActive = true;
        ShowInactive = true;
        Search = string.Empty;
    }

    private async Task ApplyFilters()
    {
        try
        {
            filteredOperationIds = await GetFilteredOperationIdsAsync();

            if (filteredOperationIds.Count > 0)
            {
                await SearchOperationAsync(filteredOperationIds);
            }
            else
            {
                StackPanelHelper.ClearAndRefreshStackPanel<CartOperationUserControl>(HomeMainContent, operationList);
                ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NotFound);
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.ERROR,
                message: "Error applying filters",
                ex: ex);

            StackPanelHelper.ClearAndRefreshStackPanel<CartOperationUserControl>(HomeMainContent, operationList);
        }
    }

    private async Task<List<double>> GetFilteredOperationIdsAsync()
    {
        var userIds = new List<double>();

        try
        {
            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                var sql = "SELECT DISTINCT id FROM public.operation WHERE 1=1";

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

    private async Task SearchOperationAsync(List<double> userIds)
    {
        if (!ManagerCookie.IsUserLoggedIn() && !ManagerCookie.IsAdministrator) return;

        if (userIds.Count == 0)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartOperationUserControl>(HomeMainContent, operationList);
            ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NotFound);
            return;
        }

        StackPanelHelper.ClearAndRefreshStackPanel<CartOperationUserControl>(HomeMainContent, operationList);

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

            string sql = $"SELECT id, name, operation_code, price, unit FROM public.operation WHERE id IN ({string.Join(", ", paramNames)})";

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
                            var viewModel = new CartOperationUserControlViewModel
                            {
                                OperationID = reader.IsDBNull(0) ? 0 : reader.GetDouble(0),
                                Name = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                                OperationCode = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                Price = reader.IsDBNull(3) ? string.Empty : reader.GetDecimal(3).ToString(),
                                Unit = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                            };

                            var userControl = new CartOperationUserControl
                            {
                                DataContext = viewModel
                            };

                            operationList.Add(userControl);
                        }
                    }
                }
            }

            StackPanelHelper.RefreshStackPanelContent<CartOperationUserControl>(HomeMainContent, operationList);

            if (operationList.Count == 0) ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NotFound);
        }
        catch (NpgsqlException ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartOperationUserControl>(HomeMainContent, operationList);
            ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NoConnectToDB);

            Loges.LoggingProcess(LogLevel.CRITICAL,
                "Connection or request error",
                ex: ex);
        }
        catch (Exception ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartOperationUserControl>(HomeMainContent, operationList);
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
            string sql = "SELECT * FROM public.operation";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var fileSave = Path.Combine(Path.Combine(Paths.DestinationPathDB("AdminPanel", "Operations")), Files.DBEquipments);

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
                            writer.WriteLine();

                            while (reader.Read())
                            {
                                for (int i = 0; i < reader.FieldCount; i++)
                                {
                                    writer.Write(reader.GetValue(i).ToString());
                                    if (i < reader.FieldCount - 1)
                                    {
                                        writer.Write(",");
                                    }
                                }
                                writer.WriteLine();
                            }
                        }

                        if (Arguments.OpenAfterDownloading) Process.Start(new ProcessStartInfo { FileName = fileSave, UseShellExecute = true });
                    }
                }
            }
        }
        catch (NpgsqlException ex)
        {
            Loges.LoggingProcess(LogLevel.ERROR,
                "Connection or request error",
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.ERROR,
                "Connection or request error",
                ex: ex);
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
