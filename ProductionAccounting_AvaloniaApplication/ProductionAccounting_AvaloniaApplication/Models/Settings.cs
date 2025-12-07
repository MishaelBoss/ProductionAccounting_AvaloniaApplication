using ProductionAccounting_AvaloniaApplication.Services.Interfaces;

namespace ProductionAccounting_AvaloniaApplication.Assets;

internal class Settings : ISettings
{
    public bool AutoUpdate { get; set; }
    public bool AutoUpdateCheck { get; set; }
    public ITimeCheckUpdate TimeCheckUpdate { get; set; } = new TimeCheckUpdate();
    public Loggers Loggers { get; set; } = new Loggers();
    public bool DeveloperDebug { get; set; }
    public bool OpenAfterDownloading { get; set; }
    public string Language { get; set; } = "ru_ru.json";
    public IServer Server { get; set; } = new Server();
}

internal class TimeCheckUpdate : ITimeCheckUpdate
{
    public int Hours { get; set; }
    public int Minutes { get; set; }
    public int Seconds { get; set; }
}

internal class Loggers : ILoggers
{
    public bool LoggerCritical { get; set; }
    public bool LoggerWarning { get; set; }
    public bool LoggerError { get; set; }
    public bool LoggerDebug { get; set; }
    public bool LoggerInfo { get; set; }
}

internal class Server : IServer
{
    public string Ip { get; set; } = string.Empty;
    public string Port { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string User { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}