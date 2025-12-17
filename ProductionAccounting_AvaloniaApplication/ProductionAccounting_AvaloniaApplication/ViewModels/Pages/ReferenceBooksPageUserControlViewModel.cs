using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProductionAccounting_AvaloniaApplication.Scripts;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class ReferenceBooksPageUserControlViewModel : ViewModelBase
{
    public ICommand OpenAddUsers
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseUserStatusMessage(true)));

    public ICommand AddProductCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseProductStatusMessage(true)));
}
