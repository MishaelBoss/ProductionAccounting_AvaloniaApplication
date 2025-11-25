using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.Views;
using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartProductUserControlViewModel : ViewModelBase
{
    public ICommand DeleteCommand
        => new RelayCommand(() =>
        {
            var confirmDeleteViewModel = new ConfirmDeleteProductWindowViewModel()
            {
                Id = ProductID,
                Name = Name,
            };

            var confirmDeleteWindows = new ConfirmDeleteProductWindow()
            {
                DataContext = confirmDeleteViewModel,
            };

            confirmDeleteViewModel.SetWindow(confirmDeleteWindows);

            confirmDeleteWindows.Show();
        });

    private double _productID = 0;
    public double ProductID 
    {
        get => _productID;
        set => this.RaiseAndSetIfChanged(ref _productID, value);
    }

    private string _name = string.Empty;
    public string Name 
    { 
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    private string _article = string.Empty;
    public string Article 
    {
        get => _article;
        set => this.RaiseAndSetIfChanged(ref _article, value);
    }

    private string _pricePerUnit = string.Empty;
    public string PricePerUnit 
    { 
        get => _pricePerUnit;
        set => this.RaiseAndSetIfChanged(ref _pricePerUnit, value);
    }

    private string _unit = string.Empty;
    public string Unit
    {
        get => _unit;
        set => this.RaiseAndSetIfChanged(ref _unit, value);
    }

    private string _coefficient = string.Empty;
    public string Coefficient
    {
        get => _coefficient;
        set => this.RaiseAndSetIfChanged(ref _coefficient, value);
    }

    public bool IsAdministrator
        => ManagerCookie.IsUserLoggedIn() && ManagerCookie.IsAdministrator;
}
