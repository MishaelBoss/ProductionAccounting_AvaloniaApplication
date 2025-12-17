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
    public string RightBoardLoginText => _localization.RightBoardLoginButton;
    public string RightBoardProfileText => _localization.RightBoardProfileButton;
    public string RightBoardSettingsText => _localization.RightBoardSettingsButton;
    public string RightBoardMainMenuText => _localization.RightBoardMainMenuTextBlock;
    #endregion
}
