using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ProductionAccounting_AvaloniaApplication.Scripts;

static class Files
{
    public const string ConfigSettingsFileName = "appsettings.json";
    public const string DbUsers = "output.csv";
    public const string DbEquipments = "output.csv";
}

static class Arguments
{
    private static readonly string? ExecPath = Process.GetCurrentProcess().MainModule?.FileName;
    private static readonly string? WorkingDir = Path.GetDirectoryName(ExecPath);
    private static readonly string? SourcePath = Path.Combine(WorkingDir ?? string.Empty, Path.GetFileName(ExecPath) ?? string.Empty);
    public static string Exenames = Path.GetFileName(SourcePath);
    public static readonly string Applicationnames = Process.GetCurrentProcess().ProcessName;

    public static bool IsAnyUserLoggedIn { get; set; } = false;

    #region Settings
    public static readonly string? CurverVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
    public static bool AutoUpdate { get; set; }
    public static bool AutoUpdateCheck { get; set; }
    public static TimeCheckUpdate TimeCheckUpdate { get; private set; } = new ();
    public static Loggers Loggers { get; private set; } = new ();
    public static bool DeveloperDebug { get; set; }
    public static bool OpenAfterDownloading { get; set; }
    public static string? PathToLocalization { get; set; }
    #endregion

    #region JsonUpdate
    public static string? NewVersion { get; set; }
    public static string? ReleaseDate { get; set; }
    public static string? DownloadUrl { get; set; }
    public static string? ChangelogUrl { get; set; }

    public static string UrlVersion
    {
        get
        {
            if (Internet.ConnectToDataBase()) return $"http://{Ip}/version.json";
            else return string.Empty;
        }
    }
    #endregion

    #region DB
    public static string? Ip { get; set; }
    public static string? Port { get; set; }
    public static string? Database { get; set; }
    public static string? User { get; set; }
    public static string? Password { get; set; }

    private static string? _connectionString;

    public static string Connection
    {
        get
        {
            if (!string.IsNullOrEmpty(_connectionString))
                return _connectionString;
                
            bool hasConnectionData =    !string.IsNullOrWhiteSpace(Ip) && 
                                        !string.IsNullOrWhiteSpace(Port) && 
                                        !string.IsNullOrWhiteSpace(Database) && 
                                        !string.IsNullOrWhiteSpace(User) && 
                                        !string.IsNullOrWhiteSpace(Password);
            
            if (!hasConnectionData)
            {
                _connectionString = Internet.ConnectToDataBase() ? $"Server={Ip};Port={Port};Database={Database};User Id={User};Password={Password};" : string.Empty;
            }
            else
            {
                _connectionString = $"Server={Ip};Port={Port};Database={Database};User Id={User};Password={Password};";
            }
            
            return _connectionString;
        }
    }

    public static void ResetConnection()
    {
        _connectionString = null;
    }
    #endregion
}

public class TimeCheckUpdate
{
    public int Hours { get; set; }
    public int Minutes { get; set; } = 1;
    public int Seconds { get; set; }
}

public class Loggers
{
    public bool LoggerCritical { get; set; }
    public bool LoggerWarning { get; set; }
    public bool LoggerError { get; set; }
    public bool LoggerDebug { get; set; }
    public bool LoggerInfo { get; set; }
}

internal static class Paths
{
    #region WorkFolders
    public const string Settings = @".\settings";
    public const string Log = @".\log";
    public static readonly string SharedFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Arguments.Applicationnames);
    private const string Db = @".\database";
    public const string Localization = @".\localization";
    public const string Temp = @".\temp";
    #endregion

    public static string DestinationPath(string directory, string name, string extension)
    {
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        return Path.Combine(directory, $"{name}.{extension}");
    }

    public static string DestinationPathDb(params string[] paths)
    {
        var allPaths = new string[paths.Length + 1];
        allPaths[0] = Db;
        Array.Copy(paths, 0, allPaths, 1, paths.Length);
        var fullPath = Path.Combine(allPaths);
        Directory.CreateDirectory(fullPath);

        return fullPath;
    }
}