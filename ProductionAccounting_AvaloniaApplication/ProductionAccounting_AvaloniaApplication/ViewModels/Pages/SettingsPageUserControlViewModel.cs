using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using ProductionAccounting_AvaloniaApplication.Scripts;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Windows.Input;

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

        CheckFileLocalization();
    }

    public ICommand TestConnectCommand
        => new RelayCommand(TestConnection);

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

    private ObservableCollection<ComboBoxLocalization> ComboBoxLocalizations { get; } = [];

    private ComboBoxLocalization? _selectedComboBoxLocalization;
    public ComboBoxLocalization? SelectedComboBoxLocalization
    {
        get => _selectedComboBoxLocalization;
        set
        {
            if (_selectedComboBoxLocalization != value)
            {
                _selectedComboBoxLocalization = value;
                OnPropertyChanged(nameof(SelectedComboBoxLocalization));
                ChangeLocalization();
            }
        }
    }

    public static ICommand RestartApplicationCommand 
        => new RelayCommand(() => 
        { 
            var processPath = Environment.ProcessPath;
            if(processPath != null)
            {
                Process.Start(processPath);
                
                if(Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop) desktop.Shutdown();
                else Environment.Exit(0);
            }
        });

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

    public void ChangeLocalization()
    {
        ManagerSettingsJson.Change("Language", SelectedComboBoxLocalization?.FileName ?? string.Empty);
    }

    private void TestConnection() 
    {
        try
        {
            if (string.IsNullOrEmpty(Ip) ||
                string.IsNullOrEmpty(Port) ||
                string.IsNullOrEmpty(Name) ||
                string.IsNullOrEmpty(User) ||
                string.IsNullOrEmpty(Password))
            {
                MessageBoxManager.GetMessageBoxStandard("Message", "Не все поля заполнены для подключения").ShowWindowAsync();
                return;
            }

            using var ping = new Ping();
            string hostName = Ip;
            PingReply reply = ping.Send(hostName, 3000);


            if (reply is { Status: IPStatus.Success })
            {
                Loges.LoggingProcess(level: LogLevel.INFO,
                    message: $"Address: {reply.Address}");

                Loges.LoggingProcess(level: LogLevel.INFO,
                    message: $"Roundtrip time: {reply.RoundtripTime}");

                Loges.LoggingProcess(level: LogLevel.INFO,
                    message: $"Time to live: {reply.Options?.Ttl}");

                TestPostgreSqlConnection();
            }
        }
        catch (PingException pex)
        {
            Loges.LoggingProcess(LogLevel.WARNING,
                $"Ping failed: {pex.Message}");
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.ERROR,
                $"Connection test failed: {ex.Message}");
        }
    }

    private void TestPostgreSqlConnection()
    {
        try
        {
            using var connection = new NpgsqlConnection($"Server={Ip};Port={Port};Database={Name};User Id={User};Password={Password};Timeout=10");
            connection.Open();

            using var cmd = new NpgsqlCommand("SELECT 1", connection);
            var result = cmd.ExecuteScalar();
            MessageBoxManager.GetMessageBoxStandard("Message", "PostgreSQL connection test successful").ShowWindowAsync();
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.ERROR,
                $"Connection test failed: {ex.Message}");
            MessageBoxManager.GetMessageBoxStandard("Message", "PostgreSQL connection failed").ShowWindowAsync();
        }
    }

    private void CheckFileLocalization()
    {
        if (!Directory.Exists(Paths.Localization)) return;

        ComboBoxLocalizations.Clear();

        foreach (string file in Directory.GetFiles(Paths.Localization, "*.json", SearchOption.AllDirectories))
        {
            if (File.Exists(file))
            {
                string fileName = Path.GetFileName(file);

                var json = File.ReadAllText(Path.Combine(Paths.Localization, fileName));
                var data = JsonSerializer.Deserialize<LocalizationData>(json);

                var listLocalization = new ComboBoxLocalization
                (
                    name: data?.Name ?? string.Empty,
                    fileName: fileName
                );

                ComboBoxLocalizations.Add(listLocalization);
            }
        }

        string currentLanguageFile = Arguments.PathToLocalization ?? string.Empty;
        
        if (!string.IsNullOrEmpty(currentLanguageFile))
        {
            foreach (var item in ComboBoxLocalizations)
            {
                if (item.FileName == currentLanguageFile)
                {
                    SelectedComboBoxLocalization = item;
                    break;
                }
            }
        }
    }


    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
