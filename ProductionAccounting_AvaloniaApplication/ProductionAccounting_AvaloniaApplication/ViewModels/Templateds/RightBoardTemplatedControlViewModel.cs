using MsBox.Avalonia.ViewModels.Commands;
using ProductionAccounting_AvaloniaApplication.ViewModels.Pages;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Templateds;

public class RightBoardTemplatedControlViewModel : ViewModelBase
{
    private MainWindowViewModel mainWindowViewModel { get; set; }

    public RightBoardTemplatedControlViewModel(MainWindowViewModel _mainWindowViewModel)
    {
        mainWindowViewModel = _mainWindowViewModel;
    }

    private readonly AdminPageTemplatedControlViewModel adminPageTemplatedControlViewModel = new();
    private readonly ProductLibraryTemplatedControlViewModel productLibraryTemplatedControlViewModel = new();
    private readonly UserLibraryTemplatedControlViewModel userLibraryTemplatedControlViewModel = new();
    private readonly WorkUserPageTemplatedControlViewModel workUserPageTemplatedControlViewModel = new();

    public ICommand OpenAdminPanelPageCommand
        => new RelayCommand((o) => OpenPage(adminPageTemplatedControlViewModel));

    public ICommand OpenProductLibraryPageCommand 
        => new RelayCommand((o) => OpenPage(productLibraryTemplatedControlViewModel));

    public ICommand OpenUserLibraryPageCommand 
        => new RelayCommand((o) => OpenPage(userLibraryTemplatedControlViewModel));

    public ICommand OpenWorkUserPageCommand 
        => new RelayCommand((o) => OpenPage(workUserPageTemplatedControlViewModel));

    private object? _objectViewModels = null;

    private void OpenPage(ViewModelBase ViewModel)
    {
        mainWindowViewModel.CurrentPage = ViewModel;
        _objectViewModels = ViewModel;
    }
}
