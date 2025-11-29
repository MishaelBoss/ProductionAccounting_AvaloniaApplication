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
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using static ProductionAccounting_AvaloniaApplication.ViewModels.Control.NotFoundUserControlViewModel;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class TabUsersListUserControlViewModel : ViewModelBase, INotifyPropertyChanged, IRecipient<OpenOrCloseProfileUserStatusMessage>, IRecipient<RefreshUserListMessage>
{
    public TabUsersListUserControlViewModel()
    {
        WeakReferenceMessenger.Default.Register<OpenOrCloseProfileUserStatusMessage>(this);
        WeakReferenceMessenger.Default.Register<RefreshUserListMessage>(this);

        _ = LoadListTypeToComboBoxAsync();
    }

    public void Receive(OpenOrCloseProfileUserStatusMessage message)
    {
        if (message.ShouldOpen)
        {
            ShowProfileUserControl(message.UserId);
            IsProfileView = true;
        }
        else 
        { 
            CloseProfileUserControl(); 
            IsProfileView = false;  
        }
    }

    public void Receive(RefreshUserListMessage message)
    {
        GetListUsers();
    }

    public StackPanel? HomeMainContent { get; set; } = null;
    public Grid? ProfileContent { get; set; } = null;

    private readonly ProfileViewUserControl _profileView = new();

    private List<CartUserListUserControl> userList = [];
    private List<double> filteredUserIds = [];

    public ICommand ResetFiltersCommand
        => new RelayCommand(() => ResetFilters());

    public ICommand DownloadAsyncCommand
        => new RelayCommand(async () => await DownloadListAsync());

    private bool _isProfileView = false;
    public bool IsProfileView
    {
        get => _isProfileView;
        set
        {
            if (_isProfileView != value)
            {
                _isProfileView = value;
                OnPropertyChanged(nameof(IsProfileView));
            }
        }
    }

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
                PerformSearchListUsers();
            }
        }
    }

    private ObservableCollection<ComboBoxTypeRolsUser> _comboBoxItems = [];
    public ObservableCollection<ComboBoxTypeRolsUser> ComboBoxItems
    {
        get => _comboBoxItems;
        set => this.RaiseAndSetIfChanged(ref _comboBoxItems, value);
    }

    private ComboBoxTypeRolsUser? _selectedComboBoxItem;
    public ComboBoxTypeRolsUser? SelectedComboBoxItem
    {
        get => _selectedComboBoxItem;
        set
        {
            if (_selectedComboBoxItem != value)
            {
                _selectedComboBoxItem = value;
                OnPropertyChanged(nameof(SelectedComboBoxItem));
                _ = ApplyFilters();
            }
        }
    }

    private ObservableCollection<ComboBoxTypeDepartmentUser> _comboBoxItemsDepartments = [];
    public ObservableCollection<ComboBoxTypeDepartmentUser> ComboBoxItemsDepartments
    {
        get => _comboBoxItemsDepartments;
        set => this.RaiseAndSetIfChanged(ref _comboBoxItemsDepartments, value);
    }

    private ComboBoxTypeDepartmentUser? _selectedComboBoxItemDepartment;
    public ComboBoxTypeDepartmentUser? SelectedComboBoxItemDepartment
    {
        get => _selectedComboBoxItemDepartment;
        set
        {
            if (_selectedComboBoxItemDepartment != value)
            {
                _selectedComboBoxItemDepartment = value;
                OnPropertyChanged(nameof(SelectedComboBoxItemDepartment));
                _ = ApplyFilters();
            }
        }
    }

    private ObservableCollection<ComboBoxTypePositionUser> _comboBoxItemsPositions = [];
    public ObservableCollection<ComboBoxTypePositionUser> ComboBoxItemsPositions
    {
        get => _comboBoxItemsPositions;
        set => this.RaiseAndSetIfChanged(ref _comboBoxItemsPositions, value);
    }

    private ComboBoxTypePositionUser? _selectedComboBoxItemPosition;
    public ComboBoxTypePositionUser? SelectedComboBoxItemPosition
    {
        get => _selectedComboBoxItemPosition;
        set
        {
            if (_selectedComboBoxItemPosition != value)
            {
                _selectedComboBoxItemPosition = value;
                OnPropertyChanged(nameof(SelectedComboBoxItemPosition));
                _ = ApplyFilters();
            }
        }
    }

    private bool _filterByRole = false;
    public bool FilterByRole
    {
        get => _filterByRole;
        set
        {
            if (_filterByRole != value)
            {
                _filterByRole = value;
                OnPropertyChanged(nameof(FilterByRole));
                _ = ApplyFilters();
            }
        }
    }

    private bool _filterByDepartment = false;
    public bool FilterByDepartment
    {
        get => _filterByDepartment;
        set
        {
            if (_filterByDepartment != value)
            {
                _filterByDepartment = value;
                OnPropertyChanged(nameof(FilterByDepartment));
                _ = ApplyFilters();
            }
        }
    }

    private bool _filterByPosition = false;
    public bool FilterByPosition
    {
        get => _filterByPosition;
        set
        {
            if (_filterByPosition != value)
            {
                _filterByPosition = value;
                OnPropertyChanged(nameof(FilterByPosition));
                _ = ApplyFilters();
            }
        }
    }

    private bool _showActiveUsers = true;
    public bool ShowActiveUsers
    {
        get => _showActiveUsers;
        set
        {
            if (_showActiveUsers != value)
            {
                _showActiveUsers = value;
                OnPropertyChanged(nameof(ShowActiveUsers));
                _ = ApplyFilters();
            }
        }
    }

    private bool _showInactiveUsers = true;
    public bool ShowInactiveUsers
    {
        get => _showInactiveUsers;
        set
        {
            if (_showInactiveUsers != value)
            {
                _showInactiveUsers = value;
                OnPropertyChanged(nameof(ShowInactiveUsers));
                _ = ApplyFilters();
            }
        }
    }

    private async void PerformSearchListUsers()
    {
        await ApplyFilters();
    }

    public async void GetListUsers()
    {
        Search = string.Empty;
        await ApplyFilters();
    }

    private async Task ApplyFilters()
    {
        try
        {
            filteredUserIds = await GetFilteredUserIdsAsync();

            if (filteredUserIds.Count > 0)
            {
                await SearchUsersAsync(filteredUserIds);
            }
            else
            {
                StackPanelHelper.ClearAndRefreshStackPanel<CartUserListUserControl>(HomeMainContent, userList);
                ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NotFound);
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.ERROR,
                message: "Error applying filters",
                ex: ex);

            StackPanelHelper.ClearAndRefreshStackPanel<CartUserListUserControl>(HomeMainContent, userList);
        }
    }

    private async Task<List<double>> GetFilteredUserIdsAsync()
    {
        var userIds = new List<double>();

        try
        {
            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                var sql = @"
                        SELECT DISTINCT u.id
                        FROM public.""user"" u
                        LEFT JOIN public.user_to_user_type utt ON u.id = utt.user_id
                        LEFT JOIN public.user_type ut ON utt.user_type_id = ut.id
                        LEFT JOIN public.user_to_departments ud ON u.id = ud.user_id
                        LEFT JOIN public.departments d ON ud.department_id = d.id
                        LEFT JOIN public.user_to_position up ON u.id = up.user_id
                        LEFT JOIN public.positions p ON up.position_id = p.id
                        WHERE 1=1";

                var parameters = new List<NpgsqlParameter>();

                if (!ShowActiveUsers || !ShowInactiveUsers)
                {
                    if (ShowActiveUsers && !ShowInactiveUsers)
                    {
                        sql += " AND u.is_active = true";
                    }
                    else if (!ShowActiveUsers && ShowInactiveUsers)
                    {
                        sql += " AND u.is_active = false";
                    }
                }

                if (!string.IsNullOrWhiteSpace(Search) && Search != "%")
                {
                    sql += " AND u.login ILIKE @search";
                    parameters.Add(new NpgsqlParameter("@search", $"%{Search}%"));
                }

                if (FilterByRole && SelectedComboBoxItem != null)
                {
                    sql += " AND ut.id = @roleId";
                    parameters.Add(new NpgsqlParameter("@roleId", SelectedComboBoxItem.Id));
                }

                if (FilterByDepartment && SelectedComboBoxItemDepartment != null)
                {
                    sql += " AND d.id = @departmentId";
                    parameters.Add(new NpgsqlParameter("@departmentId", SelectedComboBoxItemDepartment.Id));
                }

                if (FilterByPosition && SelectedComboBoxItemPosition != null)
                {
                    sql += " AND p.id = @positionId";
                    parameters.Add(new NpgsqlParameter("@positionId", SelectedComboBoxItemPosition.Id));
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

    public async Task LoadListTypeToComboBoxAsync()
    {
        try
        {
            string sqlUserTypes = "SELECT id, type_user FROM public.user_type";
            string sqlDepartments = "SELECT id, type FROM public.departments";
            string sqlPositions = "SELECT id, type FROM public.positions";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(sqlUserTypes, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        ComboBoxItems.Add(new ComboBoxTypeRolsUser(
                            reader.GetDouble(0),
                            reader.GetString(1)
                        ));
                    }
                }

                using (var command = new NpgsqlCommand(sqlDepartments, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        ComboBoxItemsDepartments.Add(new ComboBoxTypeDepartmentUser(
                            reader.GetDouble(0),
                            reader.GetString(1)
                        ));
                    }
                }

                using (var command = new NpgsqlCommand(sqlPositions, connection))
                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        ComboBoxItemsPositions.Add(new ComboBoxTypePositionUser(
                            reader.GetDouble(0),
                            reader.GetString(1)
                        ));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);
        }
    }

    private async Task SearchUsersAsync(List<double> userIds)
    {
        //if (!ManagerCookie.IsUserLoggedIn() && !ManagerCookie.IsAdministrator) return;

        if (userIds.Count == 0)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartUserListUserControl>(HomeMainContent, userList);
            ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NoConnectToDB);
            return;
        }

        StackPanelHelper.ClearAndRefreshStackPanel<CartUserListUserControl>(HomeMainContent, userList);

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

            string sql = @$"SELECT * FROM public.""user"" WHERE id IN ({string.Join(", ", paramNames)}) AND id NOT IN ({ManagerCookie.GetIdUser})";

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
                            double dbid = reader.IsDBNull(0) ? 0 : reader.GetDouble(0);
                            string dbpassword = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                            string dblogin = reader.IsDBNull(6) ? string.Empty : reader.GetString(6);
                            string dbmiddleName = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                            string dbfirst_name = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                            string dblast_name = reader.IsDBNull(3) ? string.Empty : reader.GetString(2);
                            string dbdate_joined = reader.IsDBNull(5) ? string.Empty : reader.GetDateTime(5).ToString("yyyy-MM-dd");
                            bool dbis_active = reader.IsDBNull(10) ? true : reader.GetBoolean(10);

                            var userViewModel = new CartUserListUserControlViewModel
                            {
                                UserID = dbid,
                                Login = dblogin,
                                MiddleName = dbmiddleName,
                                FirstName = dbfirst_name,
                                LastName = dblast_name,
                                Password = dbpassword,
                                DateJoined = dbdate_joined,
                                IsActive = dbis_active,
                            };

                            var userControl = new CartUserListUserControl
                            {
                                DataContext = userViewModel
                            };

                            userList.Add(userControl);
                        }
                    }
                }
            }

            StackPanelHelper.RefreshStackPanelContent<CartUserListUserControl>(HomeMainContent, userList);

            if (userList.Count == 0) ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NoConnectToDB);
        }
        catch (NpgsqlException ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartUserListUserControl>(HomeMainContent, userList);
            ItemNotFoundException.Show(HomeMainContent, ErrorLevel.NoConnectToDB);

            Loges.LoggingProcess(LogLevel.CRITICAL,
                "Connection or request error",
                ex: ex);
        }
        catch (Exception ex)
        {
            StackPanelHelper.ClearAndRefreshStackPanel<CartUserListUserControl>(HomeMainContent, userList);
            Loges.LoggingProcess(LogLevel.ERROR,
                "Error loading users by IDs",
                ex: ex);
        }
    }

    private void ResetFilters()
    {
        FilterByRole = false;
        FilterByDepartment = false;
        FilterByPosition = false;
        ShowActiveUsers = true;
        ShowInactiveUsers = true;
        SelectedComboBoxItem = null;
        SelectedComboBoxItemDepartment = null;
        SelectedComboBoxItemPosition = null;
        Search = string.Empty;
    }

    public async Task DownloadListAsync()
    {
        if (!ManagerCookie.IsUserLoggedIn()) return;

        try
        {
            string sql = "SELECT * FROM public.user";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                connection.Open();

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var fileSave = Path.Combine(Path.Combine(Paths.DestinationPathDB("AdminPanel", "UsersList")), Files.DBEquipments);

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

    public void ShowProfileUserControl(double userID)
    {
        if (ProfileContent != null)
        {
            if (ProfileContent.Children.Contains(_profileView))
            {
                _profileView.RefreshDataAsync(userID);
                return;
            }

            if (_profileView.Parent is Panel currentParent) currentParent.Children.Remove(_profileView);
            ProfileContent.Children.Clear();
            ProfileContent.Children.Add(_profileView);

            _profileView.RefreshDataAsync(userID);
        }
    }

    public void CloseProfileUserControl()
    {
        if (ProfileContent != null)
        {
            ProfileContent.Children.Clear();
            if (_profileView.Parent == ProfileContent) ProfileContent.Children.Remove(_profileView);
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
