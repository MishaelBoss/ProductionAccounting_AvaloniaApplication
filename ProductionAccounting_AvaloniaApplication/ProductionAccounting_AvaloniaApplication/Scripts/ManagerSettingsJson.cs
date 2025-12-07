using MsBox.Avalonia;
using Newtonsoft.Json;
using ProductionAccounting_AvaloniaApplication.Assets;
using System;
using System.IO;

namespace ProductionAccounting_AvaloniaApplication.Scripts;

internal abstract class ManagerSettingsJson
{
    public static void CreateSettings()
    {
        try
        {
            var defaultSettings = new
            {
                AutoUpdate = false,
                AutoUpdateCheck = true,
                TimeCheckUpdate = new
                {
                    hours = 0,
                    minutes = 1,
                    seconds = 0
                },
                Loggers = new
                {
                    loggerCritical = true,
                    loggerWarning = true,
                    loggerError = true,
                    loggerDebug = true,
                    loggerInfo = true
                },
                DeveloperDebug = false,
                OpenAfterDownloading = true,
                Language = "ru_ru.json",
                Server = new
                {
                    ip = string.Empty,
                    port = string.Empty,
                    name = string.Empty,
                    user = string.Empty,
                    password = string.Empty
                }
            };

            var dir = Paths.Settings;
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            var filePath = Path.Combine(dir, Files.ConfigSettingsFileName);
            File.WriteAllText(filePath, JsonConvert.SerializeObject(defaultSettings, Formatting.Indented));
        }
        catch (Exception ex)
        {
            MessageBoxManager.GetMessageBoxStandard("Ошибка", ex.Message).ShowAsync();
        }
    }

    public static void InitializeSettings()
    {
        try
        {
            var filePath = Path.Combine(Paths.Settings, Files.ConfigSettingsFileName);
            
            if (!File.Exists(filePath))
            {
                CreateSettings();
                return;
            }
            
            var json = File.ReadAllText(filePath);
            var data = JsonConvert.DeserializeObject<Settings>(json);

            if (data == null) return;
            
            Arguments.AutoUpdate = data.AutoUpdate;
            Arguments.AutoUpdateCheck = data.AutoUpdateCheck;
            
            if (data.TimeCheckUpdate != null)
            {
                Arguments.TimeCheckUpdate.Seconds = data.TimeCheckUpdate.Seconds;
                Arguments.TimeCheckUpdate.Minutes = data.TimeCheckUpdate.Minutes;
                Arguments.TimeCheckUpdate.Hours = data.TimeCheckUpdate.Hours;
            }

            if (data.Loggers != null)
            {
                Arguments.Loggers.LoggerCritical = data.Loggers.LoggerCritical;
                Arguments.Loggers.LoggerDebug = data.Loggers.LoggerDebug;
                Arguments.Loggers.LoggerError = data.Loggers.LoggerError;
                Arguments.Loggers.LoggerInfo = data.Loggers.LoggerInfo;
                Arguments.Loggers.LoggerWarning = data.Loggers.LoggerWarning;
            }

            Arguments.DeveloperDebug = data.DeveloperDebug;
            Arguments.OpenAfterDownloading = data.OpenAfterDownloading;
            Arguments.PathToLocalization = data.Language;

            if (data.Server != null)
            {
                Arguments.Ip = data.Server.Ip ?? string.Empty;
                Arguments.Port = data.Server.Port ?? string.Empty;
                Arguments.Database = data.Server.Name ?? string.Empty;
                Arguments.User = data.Server.User ?? string.Empty;
                Arguments.Password = data.Server.Password ?? string.Empty;
            }
        }
        catch (FileNotFoundException)
        {
            Loges.LoggingProcess(level: LogLevel.ERROR, 
                message: "file settings not found");
            CreateSettings();
        }
        catch (JsonException ex)
        {
            Loges.LoggingProcess(level: LogLevel.ERROR, 
                ex.Message);
            CreateSettings();
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.ERROR, 
                ex.Message);
        }
    }

    public static void Change(string propertyPath, object value)
    {
        try
        {
            var filePath = Path.Combine(Paths.Settings, Files.ConfigSettingsFileName);
            
            if (!File.Exists(filePath))
            {
                CreateSettings();
                return;
            }
            
            var json = File.ReadAllText(filePath);
            var data = JsonConvert.DeserializeObject<Settings>(json);

            if (data == null)
            {
                Loges.LoggingProcess(LogLevel.WARNING, "Failed to deserialize settings");
                return;
            }

            if (propertyPath.Contains('.'))
            {
                var parts = propertyPath.Split('.');
                
                if (parts.Length != 2)
                {
                    Loges.LoggingProcess(LogLevel.WARNING, 
                        $"Invalid property path: {propertyPath}");
                    return;
                }

                switch (parts[0].ToLower())
                {
                    case "server":
                        switch (parts[1].ToLower())
                        {
                            case "ip":
                                data.Server.Ip = value?.ToString() ?? string.Empty;
                                break;
                            case "port":
                                data.Server.Port = value?.ToString() ?? string.Empty;
                                break;
                            case "name":
                                data.Server.Name = value?.ToString() ?? string.Empty;
                                break;
                            case "user":
                                data.Server.User = value?.ToString() ?? string.Empty;
                                break;
                            case "password":
                                data.Server.Password = value?.ToString() ?? string.Empty;
                                break;
                            default:
                                Loges.LoggingProcess(LogLevel.WARNING,
                                    $"Unknown Server property: {parts[1]}");
                                return;
                        }
                        break;
                        
                    default:
                        Loges.LoggingProcess(LogLevel.WARNING,
                            $"Unknown object: {parts[0]}");
                        return;
                }
            }
            else
            {
                switch (propertyPath.ToLower())
                {
                    case "language":
                        data.Language = value?.ToString() ?? "ru_ru.json";
                        break;
                    case "autoupdate":
                        data.AutoUpdate = Convert.ToBoolean(value);
                        break;
                    case "autoupdatecheck":
                        data.AutoUpdateCheck = Convert.ToBoolean(value);
                        break;
                    case "developerdebug":
                        data.DeveloperDebug = Convert.ToBoolean(value);
                        break;
                    case "openafterdownloading":
                        data.OpenAfterDownloading = Convert.ToBoolean(value);
                        break;
                    default:
                        Loges.LoggingProcess(LogLevel.WARNING,
                            $"Unknown property: {propertyPath}");
                        return;
                }
            }

            var dir = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);

            File.WriteAllText(filePath, 
                JsonConvert.SerializeObject(data, Formatting.Indented));
        }
        catch (Exception ex)
        {
            MessageBoxManager.GetMessageBoxStandard("Ошибка", ex.Message).ShowAsync();
        }
    }
}
