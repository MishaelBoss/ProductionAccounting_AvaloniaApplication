using CommunityToolkit.Mvvm.Input;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartOperationAssignmentUserControlViewModel : ViewModelBase
{
    public CartOperationAssignmentUserControlViewModel(double id, Action<double> onRemove)
    {
        Id = id;
        _onRemove = onRemove;
    }

    private Action<double> _onRemove;

    private double _id;
    public double Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
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

    private decimal _plannedQuantity;
    public decimal PlannedQuantity
    {
        get => _plannedQuantity;
        set => this.RaiseAndSetIfChanged(ref _plannedQuantity, value);
    }

    public ICommand RemoveOperationCommand 
        => new RelayCommand(() => _onRemove?.Invoke(Id));

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
