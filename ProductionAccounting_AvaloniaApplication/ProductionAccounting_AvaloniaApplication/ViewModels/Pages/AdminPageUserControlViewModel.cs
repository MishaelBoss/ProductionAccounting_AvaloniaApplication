using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.View.Control;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using static ProductionAccounting_AvaloniaApplication.ViewModels.Control.NotFoundUserControlViewModel;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class AdminPageUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
    public AdminPageUserControlViewModel()
    {
        _ = LoadListTypeToComboBoxAsync();
    }

    public StackPanel? HomeMainContent { get; set; } = null;
    public StackPanel? HomeUserContent { get; set; } = null;
    public Grid? Content { get; set; } = null;

    private readonly AddUsersUserControl _addUsers = new();
    private readonly EditUsersUserControl _editUsers = new();
    private readonly AddDepartmentUserControl _addDepartment = new();
    private readonly AddPositionUserControl _addPosition = new();

    private List<CartUserListUserControl> userList = [];
    private List<double> filteredUserIds = [];

    public ICommand OpenAddUsers
        => new RelayCommand(() => ShowAddUsersUserControl());

    public ICommand OpenDepartmentPageCommand
        => new RelayCommand(() => ShowAddDepartmentUserControl());

    public ICommand OpenPositionPageCommand
        => new RelayCommand(() => ShowAddPositionUserControl());

    public ICommand ResetFiltersCommand 
        => new RelayCommand(() => ResetFilters());


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

    private double _userID = 0;
    public double UserID
    {
        get => _userID;
        set => this.RaiseAndSetIfChanged(ref _userID, value);
    }

    private string _login = string.Empty;
    public string Login
    {
        get => _login;
        set
        {
            if (_login != value)
            {
                _login = value;
                OnPropertyChanged(nameof(Login));
            }
        }
    }

    private string _firstName = string.Empty;
    public string FirstName
    {
        get => _firstName;
        set
        {
            if (_firstName != value)
            {
                _firstName = value;
                OnPropertyChanged(nameof(FirstName));
            }
        }
    }

    private string _lastName = string.Empty;
    public string LastName
    {
        get => _lastName;
        set
        {
            if (_lastName != value)
            {
                _lastName = value;
                OnPropertyChanged(nameof(LastName));
            }
        }
    }

    private string _middleName = string.Empty;
    public string MiddleName
    {
        get => _middleName;
        set
        {
            if (_middleName != value)
            {
                _middleName = value;
                OnPropertyChanged(nameof(MiddleName));
            }
        }
    }

    private string _position = string.Empty;
    public string Position
    {
        get => _position;
        set
        {
            if (_position != value)
            {
                _position = value;
                OnPropertyChanged(nameof(Position));
            }
        }
    }

    private string _employee = string.Empty;
    public string Employee
    {
        get => _employee;
        set
        {
            if (_employee != value)
            {
                _employee = value;
                OnPropertyChanged(nameof(Employee));
            }
        }
    }

    private string _department = string.Empty;
    public string Department
    {
        get => _department;
        set
        {
            if (_department != value)
            {
                _department = value;
                OnPropertyChanged(nameof(Department));
            }
        }
    }

    private string _userType = string.Empty;
    public string UserType
    {
        get => _userType;
        set
        {
            if (_userType != value)
            {
                _userType = value;
                OnPropertyChanged(nameof(UserType));
            }
        }
    }

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set
        {
            if (_email != value)
            {
                _email = value;
                OnPropertyChanged(nameof(Email));
            }
        }
    }

    private string _phone = string.Empty;
    public string Phone
    {
        get => _phone;
        set
        {
            if (_phone != value)
            {
                _phone = value;
                OnPropertyChanged(nameof(Phone));
            }
        }
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set
        {
            if (_password != value)
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
            }
        }
    }
    
    private string _isActive = string.Empty;
    public string IsActive
    {
        get => _isActive;
        set
        {
            if (_isActive != value)
            {
                _isActive = value;
                OnPropertyChanged(nameof(IsActive));
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
                ClearResults();
                ShowErrorUserControl(ErrorLevel.NotFound);
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.ERROR,
                message: "Error applying filters",
                ex: ex);

            ClearResults();
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
        if (!ManagerCookie.IsUserLoggedIn() && !ManagerCookie.IsAdministrator) return;

        if (userIds.Count == 0)
        {
            ClearResults();
            ShowErrorUserControl(ErrorLevel.NotFound);
            return;
        }

        ClearResults();

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
        
            string sql = @$"SELECT * FROM public.""user"" WHERE id IN ({string.Join(", ", paramNames)})";

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
                            string dbusername = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                            string dbfirst_name = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                            string dblast_name = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);
                            string dbdate_joined = reader.IsDBNull(5) ? string.Empty : reader.GetDateTime(5).ToString("yyyy-MM-dd");
                            bool dbis_active = reader.IsDBNull(10) ? true : reader.GetBoolean(10);

                            var userViewModel = new CartUserListUserControlViewModel
                            {
                                UserID = dbid,
                                Login = dblogin,
                                UserName = dbusername,
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

            UpdateUI();

            if (userList.Count == 0) ShowErrorUserControl(ErrorLevel.NotFound);
        }
        catch (NpgsqlException ex)
        {
            ClearResults();
            ShowErrorUserControl(ErrorLevel.NoConnectToDB);

            Loges.LoggingProcess(LogLevel.CRITICAL,
                "Connection or request error",
                ex: ex);
        }
        catch (Exception ex)
        {
            ClearResults();
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

    public async void InitDateUserAsync(double userID)
    {
        await LoadDateAsync(userID);
        await LoadListTypeToComboBoxAsync(userID);
    }

    public async Task LoadDateAsync(double userID)
    {
        if (!ManagerCookie.IsUserLoggedIn()) return;

        try
        {
            string sql = @"SELECT login, first_name, last_name, middle_name, email, phone, employee_id, is_active, password, id FROM public.""user"" WHERE id = @id";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", userID);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if(await reader.ReadAsync())
                        {
                            Login = reader.IsDBNull(0) ? "none" : reader.GetString(0);
                            FirstName = reader.IsDBNull(1) ? "none" : reader.GetString(1);
                            LastName = reader.IsDBNull(2) ? "none" : reader.GetString(2);
                            MiddleName = reader.IsDBNull(3) ? "none" : reader.GetString(3);
                            Email = reader.IsDBNull(4) ? "none" : reader.GetString(4);
                            Phone = reader.IsDBNull(5) ? "none" : reader.GetString(5);
                            Employee = reader.IsDBNull(6) ? "none" : reader.GetString(6);
                            IsActive = reader.IsDBNull(7) ? "none" : reader.GetBoolean(7).ToString();
                            Password = reader.IsDBNull(8) ? "none" : reader.GetString(8);
                            UserID = reader.IsDBNull(9) ? 0 : reader.GetDouble(9);
                        }
                    }
                }
            }
        }
        catch (NpgsqlException ex)
        {
            Loges.LoggingProcess(LogLevel.CRITICAL,
                "Connection or request error",
                ex: ex);
        }
    }
    
    public async Task LoadListTypeToComboBoxAsync(double userID)
    {
        if (!ManagerCookie.IsUserLoggedIn()) return;

        try
        {
            string sql = @"
                        SELECT 
                            ut.type_user,
                            d.type as department,
                            p.type as position
                        FROM public.user_to_user_type utt
                        LEFT JOIN public.user_type ut ON utt.user_type_id = ut.id
                        LEFT JOIN public.user_to_departments ud ON ud.user_id = utt.user_id
                        LEFT JOIN public.departments d ON ud.department_id = d.id
                        LEFT JOIN public.user_to_position up ON up.user_id = utt.user_id
                        LEFT JOIN public.positions p ON up.position_id = p.id
                        WHERE utt.user_id = @userID";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@userID", userID);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            UserType = reader.IsDBNull(0) ? "none" : reader.GetString(0);
                            Department = reader.IsDBNull(1) ? "none" : reader.GetString(1);
                            Position = reader.IsDBNull(2) ? "none" : reader.GetString(2);
                        }
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

    private void ClearResults()
    {
        userList.Clear();
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (HomeMainContent != null)
        {
            HomeMainContent.Children.Clear();
            foreach (CartUserListUserControl item in userList)
            {
                HomeMainContent.Children.Add(item);
            }
        }
    }

    public void ShowAddUsersUserControl()
    {
        if (Content != null)
        {
            if (Content.Children.Contains(_addUsers))
            {
                _ = _addUsers.RefreshDataAsync();
                return;
            }

            if (_addUsers.Parent is Panel currentParent) currentParent.Children.Remove(_addUsers);
            Content.Children.Clear();
            Content.Children.Add(_addUsers);

            _ = _addUsers.RefreshDataAsync();
        }
    }

    public void CloseAddUsersUserControl()
    {
        if (Content != null)
        {
            Content.Children.Clear();
            if (_addUsers.Parent == Content) Content.Children.Remove(_addUsers);
        }
    }

    public void ShowAddDepartmentUserControl()
    {
        if (Content != null)
        {
            if (Content.Children.Contains(_addDepartment))
            {
                _addDepartment.RefreshDataAsync();
                return;
            }

            if (_addDepartment.Parent is Panel currentParent) currentParent.Children.Remove(_addDepartment);
            Content.Children.Clear();
            Content.Children.Add(_addDepartment);

            _addDepartment.RefreshDataAsync();
        }
    }

    public void CloseAddDepartmentUserControl()
    {
        if (Content != null)
        {
            Content.Children.Clear();
            if (_addDepartment.Parent == Content) Content.Children.Remove(_addDepartment);
        }
    }

    public void ShowAddPositionUserControl()
    {
        if (Content != null)
        {
            if (Content.Children.Contains(_addPosition))
            {
                _addPosition.RefreshDataAsync();
                return;
            }

            if (_addPosition.Parent is Panel currentParent) currentParent.Children.Remove(_addPosition);
            Content.Children.Clear();
            Content.Children.Add(_addPosition);

            _addPosition.RefreshDataAsync();
        }
    }

    public void CloseAddPositionUserControl()
    {
        if (Content != null)
        {
            Content.Children.Clear();
            if (_addPosition.Parent == Content) Content.Children.Remove(_addPosition);
        }
    }

    public void ShowEditUsersUserControl(double userID)
    {
        if (Content != null)
        {
            var editViewModel = new EditUsersUserControlViewModel(userID);
            _editUsers.DataContext = editViewModel;

            if (Content.Children.Contains(_editUsers)) 
            {
                _ = _editUsers.RefreshDataAsync();
                return;
            }

            if (_editUsers.Parent is Panel currentParent) currentParent.Children.Remove(_editUsers);
            Content.Children.Clear();
            Content.Children.Add(_editUsers);

            _ = _editUsers.RefreshDataAsync();
        }
    }

    public void CloseEditUsersUserControl()
    {
        if (Content != null)
        {
            Content.Children.Clear();
            if (_editUsers.Parent == Content) Content.Children.Remove(_editUsers);
        }
    }

    private void ShowErrorUserControl(ErrorLevel level)
    {
        if (HomeMainContent != null)
        {
            var notFoundUserControlViewModel = new NotFoundUserControlViewModel(level);
            var notFoundUserControl = new NotFoundUserControl { DataContext = notFoundUserControlViewModel };

            if (HomeMainContent.Children.Contains(notFoundUserControl)) return;

            HomeMainContent.Children.Clear();
            HomeMainContent.Children.Add(notFoundUserControl);
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
 