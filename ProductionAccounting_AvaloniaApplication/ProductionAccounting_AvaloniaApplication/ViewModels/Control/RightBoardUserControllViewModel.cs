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
using JetBrains.Annotations;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class RightBoardUserControlViewModel : ViewModelBase, IRecipient<OpenOrCloseAuthorizationPageStatusMessage>, IRecipient<UserAuthenticationChangedMessage>, IRecipient<OpenOrCloseProfileUserStatusMessage>
{
    private MainWindowViewModel? MainWindowViewModel { get; }

    public RightBoardUserControlViewModel(MainWindowViewModel mainWindowViewModel)
    {
        MainWindowViewModel = mainWindowViewModel;

        UpdateUi();

        WeakReferenceMessenger.Default.Register<OpenOrCloseAuthorizationPageStatusMessage>(this);
        WeakReferenceMessenger.Default.Register<OpenOrCloseProfileUserStatusMessage>(this);
        StrongReferenceMessenger.Default.Register<UserAuthenticationChangedMessage>(this);
    }

    public void Receive(OpenOrCloseAuthorizationPageStatusMessage message)
    {
        if (message.ShouldOpen) OpenPage(_authorizationPageUserControlViewModel);
    }

    public void Receive(UserAuthenticationChangedMessage message)
    {
        UpdateUi();
    }

    public void Receive(OpenOrCloseProfileUserStatusMessage message)
    {
        ExecuteOpenProfilePage(message.UserId);
    }

    private void ExecuteOpenProfilePage(double userId)
    {
        var viewModel = new ProfilePageUserControlViewModel(userId);
        OpenPage(viewModel);
    }

    private readonly AdminPageUserControlViewModel _adminPageUserControlViewModel = new();
    private readonly WorkPageUserControlViewModel _workUserPageUserControlViewModel = new();
    private readonly SettingsPageUserControlViewModel _settingsPageUserControlViewModel = new();
    private readonly TimesheetPageUserControlViewModel _timesheetUserControlPageViewModel = new();
    private readonly ProductsManagerPageUserControlViewModel _productsManagerPageUserControlViewModel = new();
    private readonly ShipmentsPageUserControlViewModel _shipmentsPageUserControlViewModel = new();
    private readonly SalaryCalculationPageUserControlViewModel _salaryCalculationPageUserControlViewModel = new();
    private readonly AuthorizationPageUserControlViewModel _authorizationPageUserControlViewModel = new();
    private readonly ReferenceBooksPageUserControlViewModel _referenceBooksPageUserControlViewModel = new();

    [UsedImplicitly]
    public ICommand OpenAdminPanelPageCommand
        => new RelayCommand(() => OpenPage(_adminPageUserControlViewModel));

    [UsedImplicitly]
    public ICommand OpenReferenceBooksPageCommand
        => new RelayCommand(() => OpenPage(_referenceBooksPageUserControlViewModel));

    [UsedImplicitly]
    public ICommand OpenTimesheetPageCommand
        => new RelayCommand(() => OpenPage(_timesheetUserControlPageViewModel));

    [UsedImplicitly]
    public ICommand OpenWorkUserPageCommand
        => new RelayCommand(() => OpenPage(_workUserPageUserControlViewModel));

    [UsedImplicitly]
    public ICommand OpenShipmentsUserPageCommand
        => new RelayCommand(() => OpenPage(_shipmentsPageUserControlViewModel));

    [UsedImplicitly]
    public ICommand OpenSettingsUserPageCommand
        => new RelayCommand(() => OpenPage(_settingsPageUserControlViewModel));

    [UsedImplicitly]
    public ICommand OpenProductsManagerUserPageCommand
        => new RelayCommand(() => OpenPage(_productsManagerPageUserControlViewModel));

    [UsedImplicitly]
    public ICommand OpenSalaryUserPageCommand
        => new RelayCommand(() => OpenPage(_salaryCalculationPageUserControlViewModel));

    [UsedImplicitly]
    public ICommand OpenAuthorizationCommand
        => new RelayCommand(() => {
            if (ManagerCookie.IsUserLoggedIn()) ExecuteOpenProfilePage(ManagerCookie.GetIdUser ?? 0);
            else OpenPage(_authorizationPageUserControlViewModel);
        });

    private object objectViewModels;
    
    private void OpenPage(ViewModelBase viewModel)
    {
        MainWindowViewModel?.CurrentPage = viewModel;
        objectViewModels = viewModel;
    }

    private string _buttonAuthorizationText = string.Empty;
    [UsedImplicitly]
    public string ButtonAuthorizationText
    {
        get => _buttonAuthorizationText;
        set => this.RaiseAndSetIfChanged(ref _buttonAuthorizationText, value);
    }

    private ObservableCollection<DashboardButtonViewModel> _buttons = [];
    public ObservableCollection<DashboardButtonViewModel> Buttons
    { 
        get => _buttons; 
        set => this.RaiseAndSetIfChanged(ref _buttons, value); 
    }

    private bool _isAdministrator;
    [UsedImplicitly]
    public bool IsAdministrator
    {
        get => _isAdministrator;
        set => this.RaiseAndSetIfChanged(ref _isAdministrator, value);
    }

    private bool _isMaster;
    [UsedImplicitly]
    public bool IsMaster
    {
        get => _isMaster;
        set => this.RaiseAndSetIfChanged(ref _isMaster, value);
    }

    private bool _isEmployee;
    [UsedImplicitly]
    public bool IsEmployee
    {
        get => _isEmployee;
        set => this.RaiseAndSetIfChanged(ref _isEmployee, value);
    }

    private bool _isManager;
    [UsedImplicitly]
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
    [UsedImplicitly]
    public bool IsAdministratorOrManager
    {
        get => _isAdministratorOrManager;
        set => this.RaiseAndSetIfChanged(ref _isAdministratorOrManager, value);
    }

    private bool _isAdministratorOrMasterOrEmployee;
    [UsedImplicitly]
    public bool IsAdministratorOrMasterOrEmployee
    {
        get => _isAdministratorOrMasterOrEmployee;
        set => this.RaiseAndSetIfChanged(ref _isAdministratorOrMasterOrEmployee, value);
    }

    private bool _isAdministratorOrMasterOrManager;
    [UsedImplicitly]
    public bool IsAdministratorOrMasterOrManager
    {
        get => _isAdministratorOrMasterOrManager;
        set => this.RaiseAndSetIfChanged(ref _isAdministratorOrMasterOrManager, value);
    }

    private bool _isAll;
    [UsedImplicitly]
    public bool IsAll
    {
        get => _isAll;
        set => this.RaiseAndSetIfChanged(ref _isAll, value);
    }

    private void UpdateUi()
    {
        ButtonAuthorizationText = ManagerCookie.IsUserLoggedIn() ? RightBoardProfileText : RightBoardLoginText;
        var ImageProfile = ManagerCookie.IsUserLoggedIn() ? LoadBitmap("avares://ProductionAccounting_AvaloniaApplication/Assets/profile-64.png") : LoadBitmap("avares://ProductionAccounting_AvaloniaApplication/Assets/login-64.png");

        bool isLoggedIn = ManagerCookie.IsUserLoggedIn();
        IsAdministrator = isLoggedIn && ManagerCookie.IsAdministrator;
        IsMaster = isLoggedIn && ManagerCookie.IsMaster;
        IsEmployee = isLoggedIn && ManagerCookie.IsEmployee;
        IsManager = isLoggedIn && ManagerCookie.IsManager;

        IsAdministratorOrMaster = IsAdministrator || IsMaster;
        IsAdministratorOrManager = IsAdministrator || IsManager;
        IsAdministratorOrMasterOrEmployee = IsAdministrator || IsMaster || IsEmployee;
        IsAdministratorOrMasterOrManager = IsAdministrator || IsMaster || IsManager;
        IsAll = isLoggedIn;

        if (!isLoggedIn) OpenPage(_authorizationPageUserControlViewModel);
        else ExecuteOpenProfilePage(ManagerCookie.GetIdUser ?? 0);

        Buttons.Clear();

        var newButtons = new List<DashboardButtonViewModel>
        {
            new(RightBoardAdminPanelText, OpenAdminPanelPageCommand, LoadBitmap("avares://ProductionAccounting_AvaloniaApplication/Assets/administrator-64.png"), () => IsAdministrator),
            new(RightBoardManagerText, OpenProductsManagerUserPageCommand, LoadBitmap("avares://ProductionAccounting_AvaloniaApplication/Assets/administrator-64.png"), () => IsAdministratorOrManager),
            new(RightBoardTimesheetText, OpenTimesheetPageCommand, LoadBitmap("avares://ProductionAccounting_AvaloniaApplication/Assets/home-64.png"), () => IsAdministratorOrManager),
            new(RightBoardReferenceBooksText, OpenReferenceBooksPageCommand, LoadBitmap("avares://ProductionAccounting_AvaloniaApplication/Assets/library-64.png"), () => IsAll),
            new(RightBoardWindowsUserText, OpenWorkUserPageCommand, LoadBitmap("avares://ProductionAccounting_AvaloniaApplication/Assets/home-64.png"), () => IsAdministratorOrMasterOrEmployee),
            new(RightBoardShipmentsText, OpenShipmentsUserPageCommand, LoadBitmap("avares://ProductionAccounting_AvaloniaApplication/Assets/home-64.png"), () => IsAdministratorOrMasterOrManager),
            new(RightBoardSalaryText, OpenSalaryUserPageCommand, LoadBitmap("avares://ProductionAccounting_AvaloniaApplication/Assets/login-64.png"), () => IsAll),
            new(ButtonAuthorizationText, OpenAuthorizationCommand, ImageProfile),
        };

        foreach (var button in newButtons)
        {
            Buttons.Add(button);
        }

        this.RaisePropertyChanged(nameof(Buttons));
        
        foreach (var button in Buttons)
        {
            button.UpdateVisibility();
        }
    }

    private static Bitmap LoadBitmap(string uriString)
    {
        using var stream = AssetLoader.Open(new Uri(uriString));
        return new Bitmap(stream);
    }

    ~RightBoardUserControlViewModel()
    {
        StrongReferenceMessenger.Default.Unregister<UserAuthenticationChangedMessage>(this);
        WeakReferenceMessenger.Default.Unregister<OpenOrCloseAuthorizationPageStatusMessage>(this);
        WeakReferenceMessenger.Default.Unregister<OpenOrCloseProfileUserStatusMessage>(this);
    }
}