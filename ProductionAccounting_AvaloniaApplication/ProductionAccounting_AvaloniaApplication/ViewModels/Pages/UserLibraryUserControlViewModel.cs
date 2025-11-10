using Avalonia.Controls;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.View.Control;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;
using static ProductionAccounting_AvaloniaApplication.ViewModels.Control.NotFoundUserControlViewModel;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class UserLibraryUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
    public StackPanel? HomeMainContent { get; set; } = null;

    private List<CartUserListUserControl> userList = [];

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
        //if (!ManagerCookie.IsUserLoggedIn()) return;

        ClearResults();

        try
        {
            string sql = "SELECT * FROM public.\"user\" WHERE middle_name ILIKE @middle_name";

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
                            double dbid = reader.GetDouble(0);
                            string dbpassword = reader.GetString(1);
                            string dblogin = reader.GetString(6);
                            string dbusername = reader.GetString(2);
                            string dbfirst_name = reader.GetString(3);
                            string dblast_name = reader.GetString(4);
                            string dbdate_joined = reader.GetDateTime(5).ToString("yyyy-MM-dd");

                            var userViewModel = new CartUserListUserControlViewModel
                            {
                                UserID = dbid,
                                Login = dblogin,
                                UserName = dbusername,
                                FirstName = dbfirst_name,
                                LastName = dblast_name,
                                Password = dbpassword,
                                DateJoined = dbdate_joined
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
