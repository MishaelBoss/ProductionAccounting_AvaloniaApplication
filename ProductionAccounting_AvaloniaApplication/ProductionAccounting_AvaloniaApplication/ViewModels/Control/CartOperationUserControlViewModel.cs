using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartOperationUserControlViewModel : ViewModelBase
{
    private double _operationID = 0;
    public double OperationID
    {
        get => _operationID;
        set => this.RaiseAndSetIfChanged(ref _operationID, value);
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    private string _operationCode = string.Empty;
    public string OperationCode
    {
        get => _operationCode;
        set => this.RaiseAndSetIfChanged(ref _operationCode, value);
    }

    private decimal _price;
    public decimal Price
    {
        get => _price;
        set => this.RaiseAndSetIfChanged(ref _price, value);
    }

    private string _unit = string.Empty;
    public string Unit
    {
        get => _unit;
        set => this.RaiseAndSetIfChanged(ref _unit, value);
    }

    public ICommand DeleteCommand
        => new RelayCommand(() =>
        {
            string[] deleteQueries =
            [
                "DELETE FROM public.production WHERE product_id = @id"
            ];


            var viewModel = new ConfirmDeleteWindowViewModel(OperationID, Name, "DELETE FROM public.operation WHERE id = @id", (() => WeakReferenceMessenger.Default.Send(new RefreshOperationListMessage())), deleteQueries);

            var window = new ConfirmDeleteWindow()
            {
                DataContext = viewModel,
            };

            viewModel.SetWindow(window);

            window.Show();
        });

    public static bool IsAdministrator
        => ManagerCookie.IsUserLoggedIn() 
        && ManagerCookie.IsAdministrator;
}
