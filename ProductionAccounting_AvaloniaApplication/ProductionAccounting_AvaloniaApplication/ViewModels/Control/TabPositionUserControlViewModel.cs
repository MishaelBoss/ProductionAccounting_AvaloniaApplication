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

public class TabPositionUserControlViewModel : ViewModelBase, IRecipient<RefreshPositionListMessage>
{
    public TabPositionUserControlViewModel()
    {
        WeakReferenceMessenger.Default.Register<RefreshPositionListMessage>(this);
    }

    public void Receive(RefreshPositionListMessage message)
    {
        GetList();
    }

    public StackPanel? HomeMainContent { get; set; } = null;

    private List<CartPositionUserControl> positionList = [];
    private List<double> filteredPositionIds = [];

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
            filteredPositionIds = await GetFilteredPositionIdsAsync();

            if (filteredPositionIds.Count > 0)
            {
                await SearchPositionAsync(filteredPositionIds);
            }
            else
            {
                StackPanelHelper.ClearAndRefreshStackPanel<CartPositionUserControl>(HomeMainContent, positionList);
                ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NotFound);
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.ERROR,
                message: "Error applying filters",
                ex: ex);

            StackPanelHelper.ClearAndRefreshStackPanel<CartPositionUserControl>(HomeMainContent, positionList);
        }
    }

    private async Task<List<double>> GetFilteredPositionIdsAsync()
    {
        var userIds = new List<double>();

        try
        {
            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                var sql = "SELECT DISTINCT id FROM public.positions WHERE 1=1";

                var parameters = new List<NpgsqlParameter>();

                if (!string.IsNullOrWhiteSpace(Search) && Search != "%")
                {
                    sql += " AND type ILIKE @search";
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
            Loges.LoggingProcess(LogLevel.ERROR, "Error getting filtered positions IDs", ex: ex);
        }

        return userIds;
    }

    private async Task SearchPositionAsync(List<double> userIds)
    {
        if (!ManagerCookie.IsUserLoggedIn() && !ManagerCookie.IsAdministrator) return;

        if (userIds.Count == 0)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartPositionUserControl>(HomeMainContent, positionList);
            ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NotFound);
            return;
        }

        StackPanelHelper.ClearAndRefreshStackPanel<CartPositionUserControl>(HomeMainContent, positionList);

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

            string sql = $"SELECT id, type FROM public.positions operation WHERE id IN ({string.Join(", ", paramNames)})";

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
                            var viewModel = new CartPositionUserControlViewModel
                            {
                                Id = reader.IsDBNull(0) ? 0 : reader.GetDouble(0),
                                Type = reader.IsDBNull(1) ? string.Empty : reader.GetString(1)
                            };

                            var userControl = new CartPositionUserControl
                            {
                                DataContext = viewModel
                            };

                            positionList.Add(userControl);
                        }
                    }
                }
            }

            StackPanelHelper.RefreshStackPanelContent<CartPositionUserControl>(HomeMainContent, positionList);

            if (positionList.Count == 0) ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NotFound);
        }
        catch (NpgsqlException ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartPositionUserControl>(HomeMainContent, positionList);
            ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NoConnectToDB);
            Loges.LoggingProcess(LogLevel.CRITICAL,
                "Connection or request error",
                ex: ex);
        }
        catch (Exception ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartPositionUserControl>(HomeMainContent, positionList);
            ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NoConnectToDB);
            Loges.LoggingProcess(LogLevel.ERROR,
                "Error loading positions by IDs",
                ex: ex);
        }
    }

    public async Task DownloadListAsync()
    {
        if (!ManagerCookie.IsUserLoggedIn()) return;

        try
        {
            string sql = "SELECT * FROM public.positions";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var fileSave = Path.Combine(Path.Combine(Paths.DestinationPathDB("AdminPanel", "Positions")), Files.DBEquipments);

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
