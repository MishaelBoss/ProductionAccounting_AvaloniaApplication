using CommunityToolkit.Mvvm.Input;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class RightBoardUserControlViewModel : ViewModelBase
{
    private MainWindowViewModel mainWindowViewModel { get; set; }

    public RightBoardUserControlViewModel(MainWindowViewModel _mainWindowViewModel)
    {
        mainWindowViewModel = _mainWindowViewModel;
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
        => new RelayCommand(() => mainWindowViewModel.ShowAuthorization());

    private object? _objectViewModels = null;

    private void OpenPage(ViewModelBase ViewModel)
    {
        mainWindowViewModel.CurrentPage = ViewModel;
        _objectViewModels = ViewModel;
    }
}
