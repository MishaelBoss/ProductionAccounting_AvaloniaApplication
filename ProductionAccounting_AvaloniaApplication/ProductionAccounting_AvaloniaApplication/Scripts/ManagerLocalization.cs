using System;
using System.IO;
using Newtonsoft.Json;
using ProductionAccounting_AvaloniaApplication.Models;

namespace ProductionAccounting_AvaloniaApplication.Scripts;

internal abstract class ManagerLocalization
{
    private LocalizationData? _localizationData;

    public void LoadLocalization()
    {
        if (string.IsNullOrEmpty(Arguments.PathToLocalization)) return;

        try
        {
            string localizationPath = Path.Combine(Paths.Localization, Arguments.PathToLocalization);

            if (!File.Exists(localizationPath)) {
                Loges.LoggingProcess(level: LogLevel.WARNING,
                    $"Localization file not found: {localizationPath}");
                return;
            }

            var json = File.ReadAllText(localizationPath);
            var settings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.Auto,
                NullValueHandling = NullValueHandling.Ignore
            };
            _localizationData = JsonConvert.DeserializeObject<LocalizationData>(json, settings);

            if(_localizationData == null)
                Loges.LoggingProcess(level: LogLevel.WARNING,
                    "Failed to deserialize localization data");
        }
        catch (FileNotFoundException)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                "file not found json");
        }
        catch (System.Text.Json.JsonException ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.ERROR,
                ex: ex);
        }
    }

    #region Button
    #endregion

    #region Label
    #endregion

    #region TextBox
    #endregion

    #region TextBlock
    #endregion

    #region CheckBox
    #endregion
}
