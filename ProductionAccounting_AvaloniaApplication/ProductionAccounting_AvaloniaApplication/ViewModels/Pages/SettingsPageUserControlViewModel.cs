using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using CommunityToolkit.Mvvm.Input;
using MsBox.Avalonia;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using ProductionAccounting_AvaloniaApplication.Scripts;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Windows.Input;
using JetBrains.Annotations;
using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class SettingsPageUserControlViewModel : ViewModelBase
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
    [UsedImplicitly]
    public string Ip
    {
        get => _ip;
        set
        {
            this.RaiseAndSetIfChanged(ref _ip, value);
            ChangeServerIp();
        }
    }

    private string _port = string.Empty;
    [UsedImplicitly]
    public string Port
    {
        get => _port;
        set
        {
            this.RaiseAndSetIfChanged(ref _port, value);
            ChangeServerPort();
        }
    }

    private string _name = string.Empty;
    [UsedImplicitly]
    public string Name
    {
        get => _name;
        set
        {
            this.RaiseAndSetIfChanged(ref _name, value);
            ChangeServerName();
        }
    }

    private string _user = string.Empty;
    [UsedImplicitly]
    public string User
    {
        get => _user;
        set
        {
            this.RaiseAndSetIfChanged(ref _user, value);
            ChangeServerUser();
        }
    }

    private string _password = string.Empty;
    [UsedImplicitly]
    public string Password
    {
        get => _password;
        set 
        {
            this.RaiseAndSetIfChanged(ref _password, value);
            ChangeServerPassword();
        }
    }

    private ObservableCollection<ComboBoxLocalization> ComboBoxLocalizations { get; } = [];

    private ComboBoxLocalization? _selectedComboBoxLocalization;
    [UsedImplicitly]
    public ComboBoxLocalization? SelectedComboBoxLocalization
    {
        get => _selectedComboBoxLocalization;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedComboBoxLocalization, value);
            ChangeLocalization();
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

    private void ChangeServerIp()
    {
        ManagerSettingsJson.Change("Server.Ip", Ip);
    }

    private void ChangeServerPort()
    {
        ManagerSettingsJson.Change("Server.Port", Port);
    }

    private void ChangeServerName()
    {
        ManagerSettingsJson.Change("Server.Name", Name);
    }

    private void ChangeServerUser()
    {
        ManagerSettingsJson.Change("Server.User", User);
    }

    private void ChangeServerPassword()
    {
        ManagerSettingsJson.Change("Server.Password", Password);
    }

    private void ChangeLocalization()
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
                Loges.LoggingProcess(level: LogLevel.Info,
                    message: $"Address: {reply.Address}");

                Loges.LoggingProcess(level: LogLevel.Info,
                    message: $"Roundtrip time: {reply.RoundtripTime}");

                Loges.LoggingProcess(level: LogLevel.Info,
                    message: $"Time to live: {reply.Options?.Ttl}");

                TestPostgreSqlConnection();
            }
        }
        catch (PingException pex)
        {
            Loges.LoggingProcess(LogLevel.Warning,
                $"Ping failed: {pex.Message}");
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.Error,
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
            MessageBoxManager.GetMessageBoxStandard("Message", $"PostgreSQL connection test successful, result: {result}").ShowWindowAsync();
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.Error,
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
}
