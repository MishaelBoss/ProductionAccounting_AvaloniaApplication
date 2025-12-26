using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProductionAccounting_AvaloniaApplication.Scripts;
using System;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartSubProductUserControlViewModel(double id, double productTaskId, string name, string notes, decimal quantity, decimal tonnage, DateTime createAt) : ViewModelBase
{
    private double _id { get; } = id;
    private double _productTaskId { get; } = productTaskId;

    private string _name { get; } = name ?? string.Empty;
    public string Name 
    {
        get => _name;
    }

    private string _notes { get; } = notes;
    public string Notes
    {
        get => _notes;
    }


    private decimal _quantity { get; } = quantity;
    public decimal Quantity
    {
        get => _quantity;
    }

    private decimal _tonnage { get; } = tonnage;
    public decimal Tonnage
    {
        get => _tonnage;
    }

    private DateTime _createAt { get; } = createAt;
    public DateTime CreateAt
    {
        get => _createAt;
    }

    public ICommand DeleteCommand
        => new RelayCommand(() =>
        {
            var viewModel = new ConfirmDeleteWindowViewModel(id, name ?? string.Empty, "DELETE FROM public.sub_products WHERE id = @id", () => WeakReferenceMessenger.Default.Send(new RefreshSubProductListMessage()));

            var window = new ConfirmDeleteWindow()
            {
                DataContext = viewModel,
            };

            viewModel.SetWindow(window);

            window.Show();
        });

    public static bool IsAdministrator
        => ManagerCookie.IsUserLoggedIn();
}
