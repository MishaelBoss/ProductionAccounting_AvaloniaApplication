using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels;

public class ViewModelBase : ReactiveObject
{
    private readonly ManagerLocalization _localization;

    protected ViewModelBase()
    {
        _localization = new ManagerLocalization();
        _localization.LoadLocalization();
    }

    #region AuthorizationPageUserControl
    public string NameText => _localization.NameTextBlock;
    public string PasswordText => _localization.PasswordTextBlock;
    public string EnterYourNameText => _localization.EnterYourNameTextBlock;
    public string EnterYourPasswordText => _localization.EnterYourPasswordTextBlock;
    public string LoginText => _localization.LoginButton;
    #endregion

    #region RightBoardUserControl
    public string RightBoardAdminPanelText => _localization.RightBoardAdminPanelButton;
    public string RightBoardManagerText => _localization.RightBoardManagerButton;
    public string RightBoardTimesheetText => _localization.RightBoardTimesheetButton;
    public string RightBoardReferenceBooksText => _localization.RightBoardReferenceBooksButton;
    public string RightBoardWindowsUserText => _localization.RightBoardWindowsUserButton;
    public string RightBoardShipmentsText => _localization.RightBoardShipmentsButton;
    public string RightBoardSalaryText => _localization.RightBoardSalaryButton;
    public string RightBoardLoginText => _localization.RightBoardLoginButton;
    public string RightBoardProfileText => _localization.RightBoardProfileButton;
    public string RightBoardSettingsText => _localization.RightBoardSettingsButton;
    public string RightBoardMainMenuText => _localization.RightBoardMainMenuTextBlock;
    #endregion

    #region ProfilePageUserControlView
    public string ProfilePageLogoutText => _localization.ProfilePageLogoutButton;
    public string ProfilePageTablesText => _localization.ProfilePageTablesTabItem;
    public string ProfilePageProductionHistoryText => _localization.ProfilePageProductionHistoryTabItem;
    #endregion

    #region SettingsPage
    public string SettingsPageFileSettingsText => _localization.SettingsPageFileSettingsTextBlock;
    public string SettingsPageLogsTextBlock => _localization.SettingsPageLogsTextBlock;
    public string SettingsPageLanguageText => _localization.SettingsPageLanguageTextBlock;
    public string SettingsPageUpdatesText => _localization.SettingsPageUpdatesTextBlock;
    public string SettingsPageCheckForApplicationUpdatesText => _localization.SettingsPageCheckForApplicationUpdatesTextBlock;
    public string SettingsPageDatabaseConnectionText => _localization.SettingsPageDatabaseConnectionTextBlock;
    public string SettingsPageTestConnectionText => _localization.SettingsPageTestConnectionButton;
    public string SettingsPageRestartText => _localization.SettingsPageRestartButton;
    #endregion
}
