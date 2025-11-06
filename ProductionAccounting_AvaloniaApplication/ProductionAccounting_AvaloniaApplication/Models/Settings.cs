using ProductionAccounting_AvaloniaApplication.Services.Interfaces;

namespace ProductionAccounting_AvaloniaApplication.Assets;

internal class Settings(bool autoUpdate, bool autoUpdateCheck, TimeCheckUpdate timeCheckUpdate, Loggers loggers, bool developerDebug, bool openAfterDownloading, string language) : ISettings
{
    public bool AutoUpdate { get; set; } = autoUpdate;
    public bool AutoUpdateCheck { get; set; } = autoUpdateCheck;
    public ITimeCheckUpdate TimeCheckUpdate { get; set; } = timeCheckUpdate;
    public Loggers Loggers { get; set; } = loggers;
    public bool DeveloperDebug { get; set; } = developerDebug;
    public bool OpenAfterDownloading { get; set; } = openAfterDownloading;
    public string Language { get; set; } = language;
}

internal class TimeCheckUpdate(int hours, int minutes, int seconds) : ITimeCheckUpdate
{
    public int Hours { get; set; } = hours;
    public int Minutes { get; set; } = minutes;
    public int Seconds { get; set; } = seconds;
}

internal class Loggers(bool loggerCritical, bool loggerWarning, bool loggerError, bool loggerDebug, bool loggerInfo) : ILoggers
{
    public bool LoggerCritical { get; set; } = loggerCritical;
    public bool LoggerWarning { get; set; } = loggerWarning;
    public bool LoggerError { get; set; } = loggerError;
    public bool LoggerDebug { get; set; } = loggerDebug;
    public bool LoggerInfo { get; set; } = loggerInfo;
}