using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels;

public class ViewModelBase : ReactiveObject
{
    private readonly ManagerLocalization _localization;

    public ViewModelBase()
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
}
