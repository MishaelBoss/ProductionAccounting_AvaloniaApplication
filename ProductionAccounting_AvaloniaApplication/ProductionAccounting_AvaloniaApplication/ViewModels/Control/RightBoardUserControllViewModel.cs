using CommunityToolkit.Mvvm.Input;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;
using ReactiveUI;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class RightBoardUserControlViewModel : ViewModelBase
{
    private MainWindowViewModel? mainWindowViewModel { get; set; } = null;

    public RightBoardUserControlViewModel(MainWindowViewModel _mainWindowViewModel)
    {
        mainWindowViewModel = _mainWindowViewModel;

        UpdateUI();

        mainWindowViewModel?.LoginStatusChanged += OnLoginStatusChanged;
    }

    private readonly AdminPageUserControlViewModel adminPageUserControlViewModel = new();
    private readonly ProductLibraryUserControlViewModel productLibraryUserControlViewModel = new();
    private readonly UserLibraryUserControlViewModel userLibraryUserControlViewModel = new();
    private readonly WorkUserPageUserControlViewModel workUserPageUserControlViewModel = new();
    private readonly SettingsPageUserControlViewModel settingsPageUserControlViewModel = new();

    public ICommand OpenAdminPanelPageCommand
        => new RelayCommand(() => OpenPage(adminPageUserControlViewModel));

    public ICommand OpenProductLibraryPageCommand 
        => new RelayCommand(() => OpenPage(productLibraryUserControlViewModel));

    public ICommand OpenUserLibraryPageCommand 
        => new RelayCommand(() => OpenPage(userLibraryUserControlViewModel));

    public ICommand OpenWorkUserPageCommand
        => new RelayCommand(() => OpenPage(workUserPageUserControlViewModel));
        
    public ICommand OpenSettingsUserPageCommand 
        => new RelayCommand(() => OpenPage(settingsPageUserControlViewModel));

    public ICommand OpenAuthorizationCommand
        => new RelayCommand(() => {
            if (ManagerCookie.IsUserLoggedIn()) mainWindowViewModel?.ShowProfileUser();
            else mainWindowViewModel?.ShowAuthorization();
        });

    private object? _objectViewModels = null;

    private void OpenPage(ViewModelBase ViewModel)
    {
        mainWindowViewModel?.CurrentPage = ViewModel;
        _objectViewModels = ViewModel;
    }

    private string _buttonAuthorizationText = string.Empty;
    public string ButtonAuthorizationText 
    {
        get => _buttonAuthorizationText;
        set => this.RaiseAndSetIfChanged(ref _buttonAuthorizationText, value);
    }

    private bool _isVisibleAdminPanelButton;
    public bool IsVisibleAdminPanelButton
    {
        get => _isVisibleAdminPanelButton;
        set => this.RaiseAndSetIfChanged(ref _isVisibleAdminPanelButton, value);
    }

    private bool _isVisibleWorkUserButton;
    public bool IsVisibleWorkUserButton
    {
        get => _isVisibleWorkUserButton;
        set => this.RaiseAndSetIfChanged(ref _isVisibleWorkUserButton, value);
    }

    private void OnLoginStatusChanged()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (ManagerCookie.IsUserLoggedIn()) ButtonAuthorizationText = "Профиль";
        else ButtonAuthorizationText = "Войти";

        IsVisibleAdminPanelButton = ManagerCookie.IsUserLoggedIn() && ManagerCookie.IsAdministrator;
        IsVisibleWorkUserButton = ManagerCookie.IsUserLoggedIn() && (ManagerCookie.IsMaster || ManagerCookie.IsEmployee || ManagerCookie.IsAdministrator);

        if (!ManagerCookie.IsUserLoggedIn() || !ManagerCookie.IsAdministrator && _objectViewModels == adminPageUserControlViewModel) OpenPage(mainWindowViewModel);
        if (!ManagerCookie.IsUserLoggedIn() || (!ManagerCookie.IsMaster || !ManagerCookie.IsEmployee) && _objectViewModels == workUserPageUserControlViewModel) mainWindowViewModel?.ShowAuthorization();
    }
}
