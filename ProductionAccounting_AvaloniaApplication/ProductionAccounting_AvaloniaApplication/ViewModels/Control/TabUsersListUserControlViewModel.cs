using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using JetBrains.Annotations;
using static ProductionAccounting_AvaloniaApplication.ViewModels.Control.NotFoundUserControlViewModel;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class TabUsersListUserControlViewModel : ViewModelBase, IRecipient<RefreshUserListMessage>
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

    private bool _showActiveUsers = true;
    [UsedImplicitly]
    public bool ShowActiveUsers
    {
        get => _showActiveUsers;
        set
        {
            this.RaiseAndSetIfChanged(ref _showActiveUsers, value);
            _ = ScheduleSearchAsync();
        }
    }

    private bool _showInactiveUsers = true;
    [UsedImplicitly]
    public bool ShowInactiveUsers
    {
        get => _showInactiveUsers;
        set
        {
            this.RaiseAndSetIfChanged(ref _showInactiveUsers, value);
            _ = ScheduleSearchAsync();
        }
    }
    
    private CancellationTokenSource _searchCancellationTokenSource = new();
    private readonly Lock _loadingLock = new();
    private bool _isLoading;

    public TabUsersListUserControlViewModel()
    {
        WeakReferenceMessenger.Default.Register(this);

        _ = LoadUsersWithResetAsync();
    }

    public async void Receive(RefreshUserListMessage message)
    {
        try
        {
            await LoadUsersWithResetAsync();
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Critical, 
                ex: ex);
        }
    }

    public StackPanel? HomeMainContent { get; set; }

    private readonly List<CartUserListUserControl> _userList = [];

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
                    ex: ex, 
                    message: "Error download list user");
            }
        });
    
    public ICommand ReturnListUsersCommand
        => new RelayCommand(async void () =>
        {
            try
            {
                await LoadUsersWithResetAsync();
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
            
            _ = LoadUsersWithResetAsync();
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

    public async Task LoadUsersWithResetAsync()
    {
        lock (_loadingLock)
        {
            if (_isLoading) return;
            _isLoading = true;
        }

        try
        {
            ClearProductList();

            var filteredProductIds = await GetFilteredUserIdsAsync();

            if (filteredProductIds.Count > 0)  await LoadUserByIdsAsync(filteredProductIds);
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
            _userList.Clear();

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

    private async Task<List<double>> GetFilteredUserIdsAsync()
    {
        var userIds = new List<double>();

        try
        {
            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();

            var sql = "SELECT DISTINCT u.id FROM public.user u WHERE 1=1";

            var parameters = new List<NpgsqlParameter>();

            if (!ShowActiveUsers || !ShowInactiveUsers)
            {
                switch (ShowActiveUsers)
                {
                    case true when !ShowInactiveUsers:
                        sql += " AND u.is_active = true";
                        break;
                    case false when ShowInactiveUsers:
                        sql += " AND u.is_active = false";
                        break;
                }
            }

            if (!string.IsNullOrWhiteSpace(Search) && Search != "%")
            {
                sql += " AND u.login ILIKE @search";
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
            Loges.LoggingProcess(LogLevel.Error, "Error getting filtered user IDs", ex: ex);
        }

        return userIds;
    }

    private async Task LoadUserByIdsAsync(List<double> userIds)
    {
        if (userIds is { Count: 0 })
        {
            StackPanelHelper.ClearAndRefreshStackPanel(HomeMainContent, _userList);
            ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NotFound);
            return;
        }

        StackPanelHelper.ClearAndRefreshStackPanel(HomeMainContent, _userList);

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

            var sql = @$"SELECT * FROM public.user WHERE id IN ({string.Join(", ", paramNames)}) AND id NOT IN ({ManagerCookie.GetIdUser})";

            await using var connection = new NpgsqlConnection(Arguments.Connection);
            
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand(sql, connection);

            foreach (var param in parameters)
            {
                command.Parameters.Add(param);
            }

            await using var reader = await command.ExecuteReaderAsync();
                
            var newUserList = new List<CartUserListUserControl>();
                
            while (await reader.ReadAsync())
            {
                var userViewModel = new CartUserListUserControlViewModel
                {
                    UserId = reader.IsDBNull(0) ? 0 : reader.GetDouble(0),
                    Password = reader.IsDBNull(1) ? string.Empty : reader.GetString(1),
                    FirstName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                    LastName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                    MiddleName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                    DateJoined = reader.IsDBNull(5) ? string.Empty : reader.GetDateTime(5).ToString("yyyy-MM-dd"),
                    Login = reader.IsDBNull(6) ? string.Empty : reader.GetString(6),
                    BaseSalary = reader.IsDBNull(7) ? 0m : reader.GetDecimal(7),
                    Email = reader.IsDBNull(8) ? string.Empty : reader.GetString(8),
                    Phone = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                    IsActive = reader.IsDBNull(10) || reader.GetBoolean(10),
                };

                newUserList.Add(new CartUserListUserControl
                {
                    DataContext = userViewModel
                });
            }
                
            _userList.Clear();
            _userList.AddRange(newUserList);

            StackPanelHelper.RefreshStackPanelContent(HomeMainContent, _userList);

            if (_userList.Count == 0) ShowNotFoundMessage();
        }
        catch (NpgsqlException ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel(HomeMainContent, _userList);
            ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NoConnectToDb);

            Loges.LoggingProcess(LogLevel.Critical,
                "Connection or request error",
                ex: ex);
        }
        catch (Exception ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel(HomeMainContent, _userList);
            ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NoConnectToDb);
            Loges.LoggingProcess(LogLevel.Error,
                "Error loading users by IDs",
                ex: ex);
        }
    }

    private void ResetFilters()
    {
        try
        {
            ShowActiveUsers = true;
            ShowInactiveUsers = true;
            Search = string.Empty;
            
            this.RaisePropertyChanged(nameof(ShowActiveUsers));
            this.RaisePropertyChanged(nameof(ShowInactiveUsers));
            this.RaisePropertyChanged(nameof(Search));
            
            _ = LoadUsersWithResetAsync();
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
            const string sql = "SELECT * FROM public.user";

            await using var connection = new NpgsqlConnection(Arguments.Connection);
            connection.Open();

            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();
            var fileSave = Path.Combine(Path.Combine(Paths.DestinationPathDb("AdminPanel", "UsersList")), Files.DbEquipments);

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
    
    ~TabUsersListUserControlViewModel()
    {
        WeakReferenceMessenger.Default.Unregister<RefreshUserListMessage>(this);
    }
}
