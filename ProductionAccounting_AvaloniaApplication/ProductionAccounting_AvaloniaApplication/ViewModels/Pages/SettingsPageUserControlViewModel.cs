using System.ComponentModel;
using System.IO;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class SettingsPageUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
    public SettingsPageUserControlViewModel()
    {
        Ip = Arguments.Ip ?? string.Empty;
        Port = Arguments.Port ?? string.Empty;
        Name = Arguments.Database ?? string.Empty;
        User = Arguments.User ?? string.Empty;
        Password = Arguments.Password ?? string.Empty;
    }

    private string _pathToSettingsFile = Path.GetFullPath(Path.Combine(Paths.Settings, Files.ConfigSettingsFileName));
    public string PathToSettingsFile
    {
        get => _pathToSettingsFile;
        set => this.RaiseAndSetIfChanged(ref _pathToSettingsFile, value);
    }

    private string _ip = string.Empty;
    public string Ip
    {
        get => _ip;
        set
        {
            if(_ip != value)
            {
                _ip = value;
                OnPropertyChanged(nameof(Ip));
                ChangeServerIp();
            }
        }
    }

    private string _port = string.Empty;
    public string Port
    {
        get => _port;
        set
        {
            if(_port != value)
            {
                _port = value;
                OnPropertyChanged(nameof(Port));
                ChangeServerPort();
            }
        }
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set
        {
            if(_name != value)
            {
                _name = value;
                OnPropertyChanged(nameof(Name));
                ChangeServerName();
            }
        }
    }

    private string _user = string.Empty;
    public string User
    {
        get => _user;
        set
        {
            if(_user != value)
            {
                _user = value;
                OnPropertyChanged(nameof(User));
                ChangeServerUser();
            }
        }
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set
        {
            if(_password != value)
            {
                _password = value;
                OnPropertyChanged(nameof(Password));
                ChangeServerPassword();
            }
        }
    }

    // private ObservableCollection<ConfigLocalization> _comboBoxItems = [];
    // public ObservableCollection<ConfigLocalization> ComboBoxItems
    // {
    //     get => _comboBoxItems;
    //     set
    //     {
    //         if (_comboBoxItems != value)
    //         {
    //             _comboBoxItems = value;
    //             OnPropertyChanged(nameof(ComboBoxItems));
    //         }
    //     }
    // }

    // private ConfigLocalization? _selectedComboBoxItem;
    // public ConfigLocalization? SelectedComboBoxItem
    // {
    //     get => _selectedComboBoxItem;
    //     set
    //     {
    //         if (_selectedComboBoxItem != value)
    //         {
    //             _selectedComboBoxItem = value;
    //             OnPropertyChanged(nameof(SelectedComboBoxItem));
    //             SelectLanguage();
    //         }
    //     }
    // }

    // public SettingsPageViewModel() {
    //     try
    //     {
    //         string json = File.ReadAllText(Path.Combine(Paths.Localization, "config.json"));
    //         var data = JObject.Parse(json);
    //         foreach (var app in data.Properties())
    //         {
    //             if (File.Exists(Path.Combine(Paths.Localization, app.Value.ToString())))
    //             {
    //                 var localization = new ConfigLocalization(
    //                     name: app.Name,
    //                     path: app.Value.ToString()
    //                 );
    //                 ComboBoxItems.Add(localization);
    //             }
    //             else continue;
    //         }
    //     }
    //     catch (FileNotFoundException)
    //     {
    //         Loges.LoggingProcess(level: LogLevel.ERROR,
    //             "file not found json");
    //     }
    //     catch (JsonException ex)
    //     {
    //         Loges.LoggingProcess(level: LogLevel.WARNING,
    //             ex: ex);
    //     }
    //     catch (Exception ex)
    //     {
    //         Loges.LoggingProcess(level: LogLevel.ERROR,
    //             ex: ex);
    //     }
    // }

    // public void SelectLanguage() {
    //     ManagerSettingsJson.Change(SelectedComboBoxItem?.Path ?? string.Empty);
    // }

    public void ChangeServerIp()
    {
        ManagerSettingsJson.Change("Server.Ip", Ip);
    }

    public void ChangeServerPort()
    {
        ManagerSettingsJson.Change("Server.Port", Port);
    }

    public void ChangeServerName()
    {
        ManagerSettingsJson.Change("Server.Name", Name);
    }

    public void ChangeServerUser()
    {
        ManagerSettingsJson.Change("Server.User", User);
    }

    public void ChangeServerPassword()
    {
        ManagerSettingsJson.Change("Server.Password", Password);
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
