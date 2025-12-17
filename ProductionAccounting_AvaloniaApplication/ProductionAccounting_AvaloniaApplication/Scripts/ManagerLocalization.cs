using System;
using System.IO;
using Newtonsoft.Json;
using ProductionAccounting_AvaloniaApplication.Models;

namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class ManagerLocalization
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
                TypeNameHandling = TypeNameHandling.None,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
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
    public string LoginButton => _localizationData?.AuthorizationUserControl?.Button?.Login ?? string.Empty;
    public string RightBoardLoginButton => _localizationData?.RightBoardUserControl?.Button?.Login ?? string.Empty;
    public string RightBoardProfileButton => _localizationData?.RightBoardUserControl?.Button?.Profile ?? string.Empty;
    public string RightBoardSettingsButton => _localizationData?.RightBoardUserControl?.Button?.Settings ?? string.Empty;
    #endregion

    #region Label
    #endregion

    #region TextBox
    public string LogInTextBlock => _localizationData?.AuthorizationUserControl?.TextBlock?.Login ?? string.Empty;
    public string NameTextBlock => _localizationData?.AuthorizationUserControl?.TextBlock?.Name ?? string.Empty;
    public string PasswordTextBlock => _localizationData?.AuthorizationUserControl?.TextBlock?.Password ?? string.Empty;
    public string EnterYourNameTextBlock => _localizationData?.AuthorizationUserControl?.TextBlock?.EnterYourName ?? string.Empty;
    public string EnterYourPasswordTextBlock => _localizationData?.AuthorizationUserControl?.TextBlock?.EnterYourPassword ?? string.Empty;
    public string RightBoardMainMenuTextBlock => _localizationData?.RightBoardUserControl?.TextBlock?.MainMenu ?? string.Empty;
    #endregion

    #region TextBlock
    #endregion

    #region CheckBox
    #endregion
}
