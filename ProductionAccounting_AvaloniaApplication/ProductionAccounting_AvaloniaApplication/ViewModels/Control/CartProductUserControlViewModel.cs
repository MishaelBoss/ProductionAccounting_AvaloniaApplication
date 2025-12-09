using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.Views;
using ReactiveUI;
using System;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartProductUserControlViewModel : ViewModelBase
{
    public ICommand DeleteCommand
        => new RelayCommand(() =>
        {
            string[] deleteQueries =
            {
                "DELETE FROM public.production WHERE product_id = @id"
            };

            var viewModel = new ConfirmDeleteWindowViewModel(ProductId, Name, "DELETE FROM public.product WHERE id = @id", (() => WeakReferenceMessenger.Default.Send(new RefreshProductListMessage())), deleteQueries);

            var window = new ConfirmDeleteWindow()
            {
                DataContext = viewModel,
            };

            viewModel.SetWindow(window);

            window.Show();
        });

    public ICommand OpenPruductViewCommand
        => new RelayCommand(() => { WeakReferenceMessenger.Default.Send(new OpenOrCloseProductViewStatusMessage(true, Name, Id, Mark, Coefficient, Notes)); });


    private string _status = "new";
    public string Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    private string? _notes;
    public string? Notes
    {
        get => _notes;
        set => this.RaiseAndSetIfChanged(ref _notes, value);
    }

    private double? _createdBy;
    public double? CreatedBy
    {
        get => _createdBy;
        set => this.RaiseAndSetIfChanged(ref _createdBy, value);
    }

    private DateTime? _createdAt;
    public DateTime? CreatedAt
    {
        get => _createdAt;
        set => this.RaiseAndSetIfChanged(ref _createdAt, value);
    }

    private string? _productName;
    public string? ProductName
    {
        get => _productName;
        set => this.RaiseAndSetIfChanged(ref _productName, value);
    }

    private string _mark = string.Empty;
    public string Mark
    {
        get => _mark;
        set => this.RaiseAndSetIfChanged(ref _mark, value);
    }

    public string StatusDisplay => Status switch
    {
        "new" => "Новая",
        "assigned" => "Разбита на подмарки",
        "in_progress" => "В работе",
        "completed" => "Завершена",
        _ => Status
    };

    private double _id = 0;
    public double Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }

    private double _productId = 0;
    public double ProductId
    {
        get => _productId;
        set => this.RaiseAndSetIfChanged(ref _productId, value);
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

    private Int32 _pricePerUnit;
    public Int32 PricePerUnit 
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

    private Int32 _coefficient;
    public Int32 Coefficient
    {
        get => _coefficient;
        set => this.RaiseAndSetIfChanged(ref _coefficient, value);
    }
}
