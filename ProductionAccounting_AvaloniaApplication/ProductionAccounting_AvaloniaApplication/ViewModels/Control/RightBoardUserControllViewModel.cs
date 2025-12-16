using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class RightBoardUserControlViewModel : ViewModelBase, IRecipient<OpenOrCloseAuthorizationPageStatusMessage>, IRecipient<UserAuthenticationChangedMessage>
{
    private MainWindowViewModel? mainWindowViewModel { get; set; } = null;

    public RightBoardUserControlViewModel(MainWindowViewModel _mainWindowViewModel)
    {
        mainWindowViewModel = _mainWindowViewModel;

        buttons = [];

        UpdateUI();

        WeakReferenceMessenger.Default.Register<OpenOrCloseAuthorizationPageStatusMessage>(this);
        StrongReferenceMessenger.Default.Register<UserAuthenticationChangedMessage>(this);
    }

    public void Receive(OpenOrCloseAuthorizationPageStatusMessage message)
    {
        if (message.ShouldOpen) OpenPage(authorizationPageUserControlViewModel);
    }

    public void Receive(UserAuthenticationChangedMessage message)
    {
        UpdateUI();
    }

    private readonly AdminPageUserControlViewModel adminPageUserControlViewModel = new();
    private readonly ProductLibraryPageUserControlViewModel productLibraryUserControlViewModel = new();
    private readonly UserLibraryPageUserControlViewModel userLibraryUserControlViewModel = new();
    private readonly WorkPageUserControlViewModel workUserPageUserControlViewModel = new();
    private readonly SettingsPageUserControlViewModel settingsPageUserControlViewModel = new();
    private readonly TimesheetPageUserControlViewModel timesheetUserControlPageViewModel = new();
    private readonly ProductsManagerPageUserControlViewModel productsManagerPageUserControlViewModel = new();
    private readonly ShipmentsPageUserControlViewModel shipmentsPageUserControlViewModel = new();
    private readonly SalaryCalculationPageUserControlViewModel salaryCalculationPageUserControlViewModel = new();
    private readonly AuthorizationPageUserControlViewModel authorizationPageUserControlViewModel = new();

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

    public ICommand OpenSalaryUserPageCommand
        => new RelayCommand(() => OpenPage(salaryCalculationPageUserControlViewModel));

    public ICommand OpenAuthorizationCommand
        => new RelayCommand(() => {
            if (ManagerCookie.IsUserLoggedIn()) mainWindowViewModel?.ShowProfileUser();
            else OpenPage(authorizationPageUserControlViewModel);
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

    private ObservableCollection<DashboardButtonViewModel> _buttons;
    public ObservableCollection<DashboardButtonViewModel> buttons 
    { 
        get => _buttons; 
        set => this.RaiseAndSetIfChanged(ref _buttons, value); 
    }

    private bool _isAdministrator;
    public bool IsAdministrator
    {
        get => _isAdministrator;
        set => this.RaiseAndSetIfChanged(ref _isAdministrator, value);
    }

    private bool _isMaster;
    public bool IsMaster
    {
        get => _isMaster;
        set => this.RaiseAndSetIfChanged(ref _isMaster, value);
    }

    private bool _isEmployee;
    public bool IsEmployee
    {
        get => _isEmployee;
        set => this.RaiseAndSetIfChanged(ref _isEmployee, value);
    }

    private bool _isManager;
    public bool IsManager
    {
        get => _isManager;
        set => this.RaiseAndSetIfChanged(ref _isManager, value);
    }

    private bool _isAdministratorOrMaster;
    public bool IsAdministratorOrMaster
    {
        get => _isAdministratorOrMaster;
        set => this.RaiseAndSetIfChanged(ref _isAdministratorOrMaster, value);
    }

    private bool _isAdministratorOrManager;
    public bool IsAdministratorOrManager
    {
        get => _isAdministratorOrManager;
        set => this.RaiseAndSetIfChanged(ref _isAdministratorOrManager, value);
    }

    private bool _isAdministratorOrMasterOrEmployee;
    public bool IsAdministratorOrMasterOrEmployee
    {
        get => _isAdministratorOrMasterOrEmployee;
        set => this.RaiseAndSetIfChanged(ref _isAdministratorOrMasterOrEmployee, value);
    }

    private bool _isAdministratorOrMasterOrManager;
    public bool IsAdministratorOrMasterOrManager
    {
        get => _isAdministratorOrMasterOrManager;
        set => this.RaiseAndSetIfChanged(ref _isAdministratorOrMasterOrManager, value);
    }

    private bool _isAll;
    public bool IsAll
    {
        get => _isAll;
        set => this.RaiseAndSetIfChanged(ref _isAll, value);
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
        IsAdministratorOrMasterOrManager = (IsAdministrator || IsMaster || IsManager);
        IsAll = (IsAdministrator || IsMaster || IsEmployee || IsManager);

        if (!IsAdministrator && _objectViewModels == adminPageUserControlViewModel) OpenPage(authorizationPageUserControlViewModel);
        if (!IsAll && _objectViewModels == workUserPageUserControlViewModel) OpenPage(authorizationPageUserControlViewModel);

        buttons.Clear();

        var newButtons = new List<DashboardButtonViewModel>
        {
            new DashboardButtonViewModel("Admin panel", OpenAdminPanelPageCommand, LoadBitmap("avares://ProductionAccounting_AvaloniaApplication/Assets/administrator-64.png"), () => IsAdministrator),
            new DashboardButtonViewModel("Manager", OpenProductsManagerUserPageCommand, LoadBitmap("avares://ProductionAccounting_AvaloniaApplication/Assets/administrator-64.png"), () => IsAdministratorOrManager),
            new DashboardButtonViewModel("Timesheet", OpenTimesheetPageCommand, LoadBitmap("avares://ProductionAccounting_AvaloniaApplication/Assets/home-64.png"), () => IsAdministratorOrManager),
            new DashboardButtonViewModel("Library product", OpenProductLibraryPageCommand, LoadBitmap("avares://ProductionAccounting_AvaloniaApplication/Assets/library-64.png"), () => IsAll),
            new DashboardButtonViewModel("Library user", OpenUserLibraryPageCommand, LoadBitmap("avares://ProductionAccounting_AvaloniaApplication/Assets/library-64.png"), () => IsAll),
            new DashboardButtonViewModel("Windows user", OpenWorkUserPageCommand, LoadBitmap("avares://ProductionAccounting_AvaloniaApplication/Assets/home-64.png"), () => IsAdministratorOrMasterOrEmployee),
            new DashboardButtonViewModel("Shipments", OpenShipmentsUserPageCommand, LoadBitmap("avares://ProductionAccounting_AvaloniaApplication/Assets/home-64.png"), () => IsAdministratorOrMasterOrManager),
            new DashboardButtonViewModel(ButtonAuthorizationText, OpenAuthorizationCommand, LoadBitmap("avares://ProductionAccounting_AvaloniaApplication/Assets/login-64.png")),
            new DashboardButtonViewModel("Salary", OpenSalaryUserPageCommand, LoadBitmap("avares://ProductionAccounting_AvaloniaApplication/Assets/login-64.png"), () => IsAll),
        };

        foreach (var button in newButtons)
        {
            buttons.Add(button);
        }

        this.RaisePropertyChanged(nameof(buttons));
        
        foreach (var button in buttons)
        {
            button.UpdateVisibility();
        }
    }

    private Bitmap LoadBitmap(string uriString)
    {
        using (var stream = AssetLoader.Open(new Uri(uriString)))
        {
            return new Bitmap(stream);
        }
    }

    ~RightBoardUserControlViewModel()
    {
        StrongReferenceMessenger.Default.Unregister<UserAuthenticationChangedMessage>(this);
    }
}