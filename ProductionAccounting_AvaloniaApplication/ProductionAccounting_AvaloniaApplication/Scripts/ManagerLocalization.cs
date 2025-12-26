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
                Loges.LoggingProcess(level: LogLevel.Warning,
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
                Loges.LoggingProcess(level: LogLevel.Warning,
                    "Failed to deserialize localization data");
        }
        catch (FileNotFoundException)
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                "file not found json");
        }
        catch (System.Text.Json.JsonException ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Error,
                ex: ex);
        }
    }

    #region Button
    public string LoginButton => _localizationData?.AuthorizationUserControl?.Button?.Login ?? string.Empty;
    public string RightBoardLoginButton => _localizationData?.RightBoardUserControl?.Button?.Login ?? string.Empty;
    public string RightBoardProfileButton => _localizationData?.RightBoardUserControl?.Button?.Profile ?? string.Empty;
    public string RightBoardSettingsButton => _localizationData?.RightBoardUserControl?.Button?.Settings ?? string.Empty;
    public string RightBoardAdminPanelButton => _localizationData?.RightBoardUserControl?.Button?.AdminPanel ?? string.Empty;
    public string RightBoardManagerButton => _localizationData?.RightBoardUserControl?.Button?.Manager ?? string.Empty;
    public string RightBoardReferenceBooksButton => _localizationData?.RightBoardUserControl?.Button?.ReferenceBooks ?? string.Empty;
    public string RightBoardTimesheetButton => _localizationData?.RightBoardUserControl?.Button?.Timesheet ?? string.Empty;
    public string RightBoardWindowsUserButton => _localizationData?.RightBoardUserControl?.Button?.WindowsUser ?? string.Empty;
    public string RightBoardShipmentsButton => _localizationData?.RightBoardUserControl?.Button?.Shipments ?? string.Empty;
    public string RightBoardSalaryButton => _localizationData?.RightBoardUserControl?.Button?.Salary ?? string.Empty;
    public string ProfilePageLogoutButton => _localizationData?.ProfilePageUserControlView?.Button?.Logout ?? string.Empty;
    public string SettingsPageTestConnectionButton => _localizationData?.SettingsPageUserControlView?.Button?.TestConnection ?? string.Empty;
    public string SettingsPageRestartButton => _localizationData?.SettingsPageUserControlView?.Button?.Restart ?? string.Empty;
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
    public string SettingsPageFileSettingsTextBlock => _localizationData?.SettingsPageUserControlView?.TextBlock?.FileSettings ?? string.Empty;
    public string SettingsPageLogsTextBlock => _localizationData?.SettingsPageUserControlView?.TextBlock?.Logs ?? string.Empty;
    public string SettingsPageLanguageTextBlock => _localizationData?.SettingsPageUserControlView?.TextBlock?.Language ?? string.Empty;
    public string SettingsPageUpdatesTextBlock => _localizationData?.SettingsPageUserControlView?.TextBlock?.Updates ?? string.Empty;
    public string SettingsPageCheckForApplicationUpdatesTextBlock => _localizationData?.SettingsPageUserControlView?.TextBlock?.CheckForApplicationUpdates ?? string.Empty;
    public string SettingsPageDatabaseConnectionTextBlock => _localizationData?.SettingsPageUserControlView?.TextBlock?.DatabaseConnection ?? string.Empty;
    #endregion

    #region TextBlock
    #endregion

    #region TabItem
    public string ProfilePageTablesTabItem => _localizationData?.ProfilePageUserControlView?.TabItem?.Tables ?? string.Empty;
    public string ProfilePageProductionHistoryTabItem => _localizationData?.ProfilePageUserControlView?.TabItem?.ProductionHistory ?? string.Empty;
    #endregion

    #region CheckBox
    #endregion
}
