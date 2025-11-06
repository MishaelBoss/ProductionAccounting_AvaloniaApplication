using Newtonsoft.Json;

namespace ProductionAccounting_AvaloniaApplication.Services.Interfaces;

public interface ISettings
{
    [JsonProperty("AutoUpdate")]
    public bool AutoUpdate { get; set; }

    [JsonProperty("AutoUpdateCheck")]
    public bool AutoUpdateCheck { get; set; }

    [JsonProperty("TimeCheckUpdate")]
    public ITimeCheckUpdate TimeCheckUpdate { get; set; }

    [JsonProperty("DeveloperDebug")]
    public bool DeveloperDebug { get; set; }

    [JsonProperty("OpenAfterDownloading")]
    public bool OpenAfterDownloading { get; set; }
    [JsonProperty("Language")]
    public string Language { get; set; }
}

public interface ITimeCheckUpdate
{
    [JsonProperty("hours")]
    public int Hours { get; set; }

    [JsonProperty("minutes")]
    public int Minutes { get; set; }

    [JsonProperty("seconds")]
    public int Seconds { get; set; }
}

public interface ILoggers
{
    [JsonProperty("loggerCritical")]
    public bool LoggerCritical { get; set; }

    [JsonProperty("loggerWarning")]
    public bool LoggerWarning { get; set; }

    [JsonProperty("loggerError")]
    public bool LoggerError { get; set; }

    [JsonProperty("loggerDebug")]
    public bool LoggerDebug { get; set; }

    [JsonProperty("loggerInfo")]
    public bool LoggerInfo { get; set; }
}