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
            var defaultData = new
            {
                AutoUpdate = false,
                AutoUpdateCheck = true,
                TimeCheckUpdate = new
                {
                    hours = 0,
                    minutes = 1,
                    seconds = 0
                },
                TimeAllUpdateUI = new
                {
                    hours = 0,
                    minutes = 0,
                    seconds = 5
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
                Language = "ru_ru.json"
            };

            if (Directory.Exists(Paths.Settings)) File.WriteAllText(Path.Combine(Paths.Settings, Files.ConfigSettingsFileName), JsonConvert.SerializeObject(defaultData, Formatting.Indented));
            else Directory.CreateDirectory(Paths.Settings);
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
            var json = File.ReadAllText(Path.Combine(Paths.Settings, Files.ConfigSettingsFileName));
            var data = JsonConvert.DeserializeObject<Settings>(json);

            if (data == null) return;
            Arguments.AutoUpdate = data.AutoUpdate;
            Arguments.AutoUpdateCheck = data.AutoUpdateCheck;

            if (data.TimeCheckUpdate is { Seconds: 0, Minutes: 0, Hours: 0 })
            {
                Arguments.TimeCheckUpdate.Seconds = data.TimeCheckUpdate.Seconds;
                Arguments.TimeCheckUpdate.Minutes = 1;
                Arguments.TimeCheckUpdate.Hours = data.TimeCheckUpdate.Hours;
            }
            else
            {
                Arguments.TimeCheckUpdate.Seconds = data.TimeCheckUpdate.Seconds;
                Arguments.TimeCheckUpdate.Minutes = data.TimeCheckUpdate.Minutes;
                Arguments.TimeCheckUpdate.Hours = data.TimeCheckUpdate.Hours;
            }

            if ((data.TimeCheckUpdate is { Seconds: >= 0, Minutes: >= 0, Hours: >= 0 }) &
                Arguments.DeveloperDebug)
                MessageBoxManager.GetMessageBoxStandard("Error", "i < 0").ShowAsync();

            Arguments.Loggers.LoggerCritical = data.Loggers.LoggerCritical;
            Arguments.Loggers.LoggerDebug = data.Loggers.LoggerDebug;
            Arguments.Loggers.LoggerError = data.Loggers.LoggerError;
            Arguments.Loggers.LoggerInfo = data.Loggers.LoggerInfo;
            Arguments.Loggers.LoggerWarning = data.Loggers.LoggerWarning;

            Arguments.DeveloperDebug = data.DeveloperDebug;
            Arguments.OpenAfterDownloading = data.OpenAfterDownloading;

            Arguments.PathToLocalization = data.Language;
        }
        catch (FileNotFoundException)
        {
            MessageBoxManager.GetMessageBoxStandard("Ошибка", "файл к json не найден").ShowAsync();
        }
        catch (JsonException ex)
        {
            MessageBoxManager.GetMessageBoxStandard("Ошибка", ex.Message).ShowAsync();
        }
        catch (Exception ex)
        {
            MessageBoxManager.GetMessageBoxStandard("Ошибка", ex.Message).ShowAsync();
        }
    }

    public static void Change(string dsad)
    {
        try
        {
            var json = File.ReadAllText(Path.Combine(Paths.Settings, Files.ConfigSettingsFileName));
            var data = JsonConvert.DeserializeObject<Settings>(json);

            if(data == null)
                Loges.LoggingProcess(level: LogLevel.WARNING,
                   "Failed to deserialize localization data");

            data?.Language = dsad;
            File.WriteAllText(Path.Combine(Paths.Settings, Files.ConfigSettingsFileName), JsonConvert.SerializeObject(data, Formatting.Indented));
        }
        catch (Exception ex)
        {
            MessageBoxManager.GetMessageBoxStandard("Ошибка", ex.Message).ShowAsync();
        }
    }
}
