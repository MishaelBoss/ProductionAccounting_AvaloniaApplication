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
    private readonly ProductLibraryPageUserControlViewModel productLibraryUserControlViewModel = new();
    private readonly UserLibraryPageUserControlViewModel userLibraryUserControlViewModel = new();
    private readonly WorkPageUserControlViewModel workUserPageUserControlViewModel = new();
    private readonly SettingsPageUserControlViewModel settingsPageUserControlViewModel = new();
    private readonly TimesheetPageUserControlViewModel timesheetUserControlPageViewModel = new();
    private readonly ProductsManagerPageUserControlViewModel productsManagerPageUserControlViewModel = new();
    private readonly ShipmentsPageUserControlViewModel shipmentsPageUserControlViewModel = new();

    public ICommand OpenAdminPanelPageCommand
        => new RelayCommand(() => OpenPage(adminPageUserControlViewModel));

    public ICommand OpenProductLibraryPageCommand
        => new RelayCommand(() => OpenPage(productLibraryUserControlViewModel));

    public ICommand OpenUserLibraryPageCommand
        => new RelayCommand(() => OpenPage(userLibraryUserControlViewModel));

    public ICommand OpenTimesheetPageCommand
        => new RelayCommand(() => OpenPage(timesheetUserControlPageViewModel));

    public ICommand OpenWorkUserPageCommand
        => new RelayCommand(() => OpenPage(workUserPageUserControlViewModel));

    public ICommand OpenShipmentsUserPageCommand
        => new RelayCommand(() => OpenPage(shipmentsPageUserControlViewModel));

    public ICommand OpenSettingsUserPageCommand
        => new RelayCommand(() => OpenPage(settingsPageUserControlViewModel));

    public ICommand OpenProductsManagerUserPageCommand
        => new RelayCommand(() => OpenPage(productsManagerPageUserControlViewModel));

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

    private void OnLoginStatusChanged()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (ManagerCookie.IsUserLoggedIn()) ButtonAuthorizationText = "Профиль";
        else ButtonAuthorizationText = "Войти";

        IsAdministrator = ManagerCookie.IsUserLoggedIn() && ManagerCookie.IsAdministrator;
        IsMaster = ManagerCookie.IsUserLoggedIn() && ManagerCookie.IsMaster;
        IsEmployee = ManagerCookie.IsUserLoggedIn() && ManagerCookie.IsEmployee;
        IsManager = ManagerCookie.IsUserLoggedIn() && ManagerCookie.IsManager;

        IsAdministratorOrMaster = (IsAdministrator || IsMaster);
        IsAdministratorOrManager = (IsAdministrator || IsManager);
        IsAdministratorOrMasterOrEmployee = (IsAdministrator || IsMaster || IsEmployee);
        IsAdministratorOrMasterOrManager = (IsAdministrator || IsMaster || IsMaster);
        IsAll = (IsAdministrator || IsMaster || IsEmployee || IsManager);

        if (!IsAdministrator && _objectViewModels == adminPageUserControlViewModel) OpenPage(mainWindowViewModel);
        if (!IsAll && _objectViewModels == workUserPageUserControlViewModel) mainWindowViewModel?.ShowAuthorization();
    }
}
