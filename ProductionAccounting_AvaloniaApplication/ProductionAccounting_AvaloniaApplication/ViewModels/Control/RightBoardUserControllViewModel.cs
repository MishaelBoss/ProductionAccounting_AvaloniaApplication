using CommunityToolkit.Mvvm.Input;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;
using ReactiveUI;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class RightBoardUserControlViewModel : ViewModelBase
{
    private MainWindowViewModel mainWindowViewModel { get; set; }

    public RightBoardUserControlViewModel(MainWindowViewModel _mainWindowViewModel)
    {
        mainWindowViewModel = _mainWindowViewModel;

        UpdateUI();

        mainWindowViewModel?.LoginStatusChanged += OnLoginStatusChanged;
    }

    private readonly AdminPageUserControlViewModel adminPageTemplatedControlViewModel = new();
    private readonly ProductLibraryUserControlViewModel productLibraryTemplatedControlViewModel = new();
    private readonly UserLibraryUserControlViewModel userLibraryTemplatedControlViewModel = new();
    private readonly WorkUserPageUserControlViewModel workUserPageTemplatedControlViewModel = new();

    public ICommand OpenAdminPanelPageCommand
        => new RelayCommand(() => OpenPage(adminPageTemplatedControlViewModel));

    public ICommand OpenProductLibraryPageCommand 
        => new RelayCommand(() => OpenPage(productLibraryTemplatedControlViewModel));

    public ICommand OpenUserLibraryPageCommand 
        => new RelayCommand(() => OpenPage(userLibraryTemplatedControlViewModel));

    public ICommand OpenWorkUserPageCommand 
        => new RelayCommand(() => OpenPage(workUserPageTemplatedControlViewModel));

    public ICommand OpenAuthorizationCommand
        => new RelayCommand(() => {
            if (ManagerCookie.IsUserLoggedIn()) {
                ManagerCookie.DeleteCookie();
                mainWindowViewModel.NotifyLoginStatusChanged();
            }
            else mainWindowViewModel.ShowAuthorization();
        });

    private object? _objectViewModels = null;

    private void OpenPage(ViewModelBase ViewModel)
    {
        mainWindowViewModel.CurrentPage = ViewModel;
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
        if (ManagerCookie.IsUserLoggedIn()) ButtonAuthorizationText = "Выйти";
        else ButtonAuthorizationText = "Войти";

        IsVisibleAdminPanelButton = ManagerCookie.IsUserLoggedIn() && ManagerCookie.IsAdministrator;
        IsVisibleWorkUserButton = ManagerCookie.IsUserLoggedIn() && (ManagerCookie.IsMaster || ManagerCookie.IsEmployee);

        if (!ManagerCookie.IsUserLoggedIn() || !ManagerCookie.IsAdministrator && _objectViewModels == adminPageTemplatedControlViewModel) OpenPage(workUserPageTemplatedControlViewModel);
        if (!ManagerCookie.IsUserLoggedIn() || (!ManagerCookie.IsMaster || !ManagerCookie.IsEmployee) && _objectViewModels == workUserPageTemplatedControlViewModel) mainWindowViewModel.ShowAuthorization();
    }
}
