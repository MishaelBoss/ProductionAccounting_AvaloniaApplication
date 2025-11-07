using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace ProductionAccounting_AvaloniaApplication.Scripts;

class Files
{
    public const string ConfigSettingsFileName = "appsettings.json";
    public const string DBUsers = "output.csv";
    public const string DBEquipments = "output.csv";
}

class Arguments
{
    private static readonly string? ExecPath = Process.GetCurrentProcess()?.MainModule?.FileName;
    private static readonly string? WorkingDir = Path.GetDirectoryName(ExecPath);
    private static readonly string? SourcePath = Path.Combine(WorkingDir ?? string.Empty, Path.GetFileName(ExecPath) ?? string.Empty);
    public static string Exenames = Path.GetFileName(SourcePath);
    public static string Applicationnames = Process.GetCurrentProcess().ProcessName;

    public static bool IsAnyUserLoggedIn { get; set; } = false;

    #region Settings
    public static readonly string? CurverVersion = Assembly.GetExecutingAssembly()?.GetName()?.Version?.ToString();
    public static bool AutoUpdate { get; set; }
    public static bool AutoUpdateCheck { get; set; }
    public static TimeCheckUpdate TimeCheckUpdate { get; private set; } = new TimeCheckUpdate();
    public static Loggers Loggers { get; private set; } = new Loggers();
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
            if (Internet.ConnectToDataBase()) return "http://192.168.0.108:8000/version.json";
            else return string.Empty;
        }
    }
    #endregion

    #region DB
    private static string? Ip { get; set; }
    private static string? Port { get; set; }
    private static string? Database { get; set; }
    private static string? UserId { get; set; }
    private static string? Password { get; set; }

    private static void Connect()
    {
        if (Internet.ConnectToDataBase())
        {
            Ip = "192.168.0.108";
            Port = "5432";
            Database = "ProductionAccounting";
            UserId = "postgres";
            Password = "cr2032";
        }
        else
        {
            Ip = "localhost";
            Port = "5432";
            Database = "ProductionAccounting";
            UserId = "postgres";
            Password = "cr2032";
        }
    }

    public static string connection
    {
        get
        {
            if (string.IsNullOrWhiteSpace(Ip) && string.IsNullOrWhiteSpace(Port) && string.IsNullOrWhiteSpace(Database) && string.IsNullOrWhiteSpace(UserId) && string.IsNullOrWhiteSpace(Password)) Connect();
            return $"Server={Ip};Port={Port};Database={Database};User Id={UserId};Password={Password};";
        }
    }
    #endregion
}

class TimeCheckUpdate
{
    public int Hours { get; set; } = 0;
    public int Minutes { get; set; } = 1;
    public int Seconds { get; set; } = 0;
}

class Loggers
{
    public bool LoggerCritical { get; set; }
    public bool LoggerWarning { get; set; }
    public bool LoggerError { get; set; }
    public bool LoggerDebug { get; set; }
    public bool LoggerInfo { get; set; }
}

class Paths
{
    #region WorkFolders
    public const string Settings = @".\settings";
    public const string Log = @".\log";
    public static string SharedFolder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), Arguments.Applicationnames);
    private const string DB = @".\database";
    public const string Localization = @".\localization";
    public const string Temp = @".\temp";
    #endregion

    public static string DestinationPath(string directory, string name, string extension)
    {
        if (!Directory.Exists(directory)) Directory.CreateDirectory(directory);
        return Path.Combine(directory, $"{name}.{extension}");
    }

    public static string DestinationPathDB(string name)
    {
        if (!Directory.Exists(Path.Combine(DB, name))) Directory.CreateDirectory(Path.Combine(DB, $"{name}"));
        return Path.Combine(DB, name);
    }
}