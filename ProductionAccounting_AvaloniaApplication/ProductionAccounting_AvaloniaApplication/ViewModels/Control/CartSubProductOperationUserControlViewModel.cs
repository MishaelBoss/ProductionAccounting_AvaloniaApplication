using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartSubProductOperationUserControlViewModel : ViewModelBase
{

    private double? _id;
    public double? Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }

    private double? _subProductId;
    public double? SubProductId
    {
        get => _subProductId;
        set => this.RaiseAndSetIfChanged(ref _subProductId, value);
    }

    private double? _operationId;
    public double? OperationId
    {
        get => _operationId;
        set => this.RaiseAndSetIfChanged(ref _operationId, value);
    }

    private decimal? _plannedQuantity;
    public decimal? PlannedQuantity
    {
        get => _plannedQuantity;
        set => this.RaiseAndSetIfChanged(ref _plannedQuantity, value);
    }

    private string? _status;
    public string? Status
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

    private string? _operationName;
    public string? OperationName
    {
        get => _operationName;
        set => this.RaiseAndSetIfChanged(ref _operationName, value);
    }

    private string? _unit;
    public string? Unit
    {
        get => _unit;
        set => this.RaiseAndSetIfChanged(ref _unit, value);
    }

    private decimal? _price;
    public decimal? Price
    {
        get => _price;
        set => this.RaiseAndSetIfChanged(ref _price, value);
    }
}
