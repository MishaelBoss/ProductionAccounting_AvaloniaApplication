using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.View.Control;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using static ProductionAccounting_AvaloniaApplication.ViewModels.Control.NotFoundUserControlViewModel;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class AdminPageUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
    public StackPanel? HomeMainContent { get; set; } = null;
    public StackPanel? HomeUserContent { get; set; } = null;
    public Grid? Content { get; set; } = null;

    private readonly AddUsersUserControl _addUsers = new();
    private readonly EditUsersUserControl _editUsers = new();
    private readonly AddDepartmentUserControl _addDepartment = new();
    private readonly AddPositionUserControl _addPosition = new();

    private List<CartUserListUserControl> userList = [];

    public ICommand OpenAddUsers
        => new RelayCommand(() => ShowAddUsersUserControl());

    public ICommand OpenDepartmentPageCommand
        => new RelayCommand(() => ShowAddDepartmentUserControl());

    public ICommand OpenPositionPageCommand
        => new RelayCommand(() => ShowAddPositionUserControl());

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

    private async void PerformSearchListUsers()
    {
        if (string.IsNullOrWhiteSpace(Search))
        {
            GetListUsers();
            return;
        }

        await SearchUsersAsync($"%{Search}%");
    }

    public async void GetListUsers()
    {
        await SearchUsersAsync("%");
    }

    private async Task SearchUsersAsync(string search)
    {
        if (!ManagerCookie.IsUserLoggedIn() && !ManagerCookie.IsAdministrator) return;

        ClearResults();

        try
        {
            string sql = "SELECT * FROM public.\"user\" WHERE middle_name ILIKE @middle_name AND is_active IN (true, false)";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@middle_name", search);

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

            if (Content.Children.Contains(_editUsers)) return;

            if (_editUsers.Parent is Panel currentParent) currentParent.Children.Remove(_editUsers);
            Content.Children.Clear();
            Content.Children.Add(_editUsers);
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
