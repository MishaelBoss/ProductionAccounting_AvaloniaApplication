using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using ReactiveUI;
using static ProductionAccounting_AvaloniaApplication.ViewModels.Control.NotFoundUserControlViewModel;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class TabPositionUserControlViewModel : ViewModelBase, IRecipient<RefreshPositionListMessage>
{
    private string _search = string.Empty;
    [UsedImplicitly]
    public string Search
    {
        get => _search;
        set
        {
            this.RaiseAndSetIfChanged(ref _search, value);
            this.RaisePropertyChanged(nameof(PerformSearchList));
        }
    }

    public TabPositionUserControlViewModel()
    {
        WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(RefreshPositionListMessage message)
    {
        GetList();
    }

    public StackPanel? HomeMainContent { get; set; }

    private readonly List<CartPositionUserControl> _positionList = [];
    private List<double> _filteredPositionIds = [];

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
                    ex: ex, 
                    message: "Error download");
            }
        });

    public ICommand RefreshAsyncCommand
        => new RelayCommand(GetList);

    private async void PerformSearchList()
    {
        try
        {
            await ApplyFilters();
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Error,
                ex: ex);
        }
    }

    public async void GetList()
    {
        try
        {
            Search = string.Empty;
            await ApplyFilters();
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Error,
                ex: ex);
        }
    }

    private async Task ApplyFilters()
    {
        try
        {
            _filteredPositionIds = await GetFilteredPositionIdsAsync();

            if (_filteredPositionIds.Count > 0)
            {
                await SearchPositionAsync(_filteredPositionIds);
            }
            else
            {
                StackPanelHelper.ClearAndRefreshStackPanel(HomeMainContent, _positionList);
                ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NotFound);
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Error,
                message: "Error applying filters",
                ex: ex);

            StackPanelHelper.ClearAndRefreshStackPanel(HomeMainContent, _positionList);
        }
    }

    private async Task<List<double>> GetFilteredPositionIdsAsync()
    {
        var userIds = new List<double>();

        try
        {
            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();

            var sql = "SELECT DISTINCT id FROM public.positions WHERE 1=1";

            var parameters = new List<NpgsqlParameter>();

            if (!string.IsNullOrWhiteSpace(Search) && Search != "%")
            {
                sql += " AND type ILIKE @search";
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
                    userIds.Add(reader.GetDouble(0));
                }
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.Error, "Error getting filtered positions IDs", ex: ex);
        }

        return userIds;
    }

    private async Task SearchPositionAsync(List<double> userIds)
    {
        if (!ManagerCookie.IsUserLoggedIn() && !ManagerCookie.IsAdministrator) return;

        if (userIds.Count == 0)
        {
            StackPanelHelper.ClearAndRefreshStackPanel(HomeMainContent, _positionList);
            ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NotFound);
            return;
        }

        StackPanelHelper.ClearAndRefreshStackPanel(HomeMainContent, _positionList);

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

            await using (var connection = new NpgsqlConnection(Arguments.Connection))
            {
                await connection.OpenAsync();
                await using var command = new NpgsqlCommand(sql, connection);

                foreach (var param in parameters)
                {
                    command.Parameters.Add(param);
                }

                await using var reader = await command.ExecuteReaderAsync();
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

                    _positionList.Add(userControl);
                }
            }

            StackPanelHelper.RefreshStackPanelContent(HomeMainContent, _positionList);

            if (_positionList.Count == 0) ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NotFound);
        }
        catch (NpgsqlException ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel(HomeMainContent, _positionList);
            ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NoConnectToDb);
            Loges.LoggingProcess(LogLevel.Critical,
                "Connection or request error",
                ex: ex);
        }
        catch (Exception ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel(HomeMainContent, _positionList);
            ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NoConnectToDb);
            Loges.LoggingProcess(LogLevel.Error,
                "Error loading positions by IDs",
                ex: ex);
        }
    }

    private static async Task DownloadListAsync()
    {
        if (!ManagerCookie.IsUserLoggedIn()) return;

        try
        {
            const string sql = "SELECT * FROM public.positions";

            await using var connection = new NpgsqlConnection(Arguments.Connection);
            connection.Open();

            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();
            var fileSave = Path.Combine(Path.Combine(Paths.DestinationPathDb("AdminPanel", "Positions")), Files.DbEquipments);

            await using (var writer = new StreamWriter(fileSave))
            {
                for (int i = 0; i < reader.FieldCount; i++)
                {
                    await writer.WriteAsync(reader.GetName(i));
                    if (i < reader.FieldCount - 1)
                    {
                        await writer.WriteAsync(",");
                    }
                }
                await writer.WriteLineAsync();

                while (reader.Read())
                {
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        await writer.WriteAsync(reader.GetValue(i).ToString());
                        if (i < reader.FieldCount - 1)
                        {
                            await writer.WriteAsync(",");
                        }
                    }
                    await writer.WriteLineAsync();
                }
            }

            if (Arguments.OpenAfterDownloading) Process.Start(new ProcessStartInfo { FileName = fileSave, UseShellExecute = true });
        }
        catch (NpgsqlException ex)
        {
            Loges.LoggingProcess(LogLevel.Error,
                "Connection or request error",
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.Error,
                "Connection or request error",
                ex: ex);
        }
    }
}
