using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartSubProductUserControlViewModel : ViewModelBase
{
    public ICommand DeleteCommand
        => new RelayCommand(() =>
        {
            var viewModel = new ConfirmDeleteWindowViewModel(Id ?? 0, Name ?? string.Empty, "DELETE FROM public.sub_products WHERE id = @id", (() => WeakReferenceMessenger.Default.Send(new RefreshSubProductListMessage())));

            var window = new ConfirmDeleteWindow()
            {
                DataContext = viewModel,
            };

            viewModel.SetWindow(window);

            window.Show();
        });

    public ICommand ViewCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseSubProductStatusMessage(true, Id)));

    private double? _id;
    public double? Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }

    private double? _productTaskId;
    public double? ProductTaskId
    {
        get => _productTaskId;
        set => this.RaiseAndSetIfChanged(ref _productTaskId, value);
    }

    private string? _name;
    public string? Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    private double? _plannedQuantity;
    public double? PlannedQuantity
    {
        get => _plannedQuantity;
        set => this.RaiseAndSetIfChanged(ref _plannedQuantity, value);
    }

    private double? _plannedWeight;
    public double? PlannedWeight
    {
        get => _plannedWeight;
        set => this.RaiseAndSetIfChanged(ref _plannedWeight, value);
    }

    private string? _notes;
    public string? Notes
    {
        get => _notes;
        set => this.RaiseAndSetIfChanged(ref _notes, value);
    }

    private DateTime? _createAt;
    public DateTime? CreateAt
    {
        get => _createAt;
        set => this.RaiseAndSetIfChanged(ref _createAt, value);
    }

    private List<CartOperationUserControl> _operations = [];
    public List<CartOperationUserControl> Operations 
    {
        get => _operations;
        set => this.RaiseAndSetIfChanged(ref _operations, value);
    }

    public bool IsNullNotes
        => !string.IsNullOrEmpty(Notes);

    public bool IsAdministrator
        => ManagerCookie.IsUserLoggedIn()
        && ManagerCookie.IsAdministrator;
}
