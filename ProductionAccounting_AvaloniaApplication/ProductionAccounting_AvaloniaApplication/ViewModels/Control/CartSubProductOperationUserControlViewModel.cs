using Avalonia.Media;
using CommunityToolkit.Mvvm.Messaging;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartSubProductOperationUserControlViewModel : ViewModelBase
{
    private double? _operationId;
    public double? OperationId
    {
        get => _operationId;
        set => this.RaiseAndSetIfChanged(ref _operationId, value);
    }

    private string? _operationName;
    public string? OperationName
    {
        get => _operationName;
        set => this.RaiseAndSetIfChanged(ref _operationName, value);
    }

    private decimal? _plannedQuantity;
    public decimal? PlannedQuantity
    {
        get => _plannedQuantity;
        set => this.RaiseAndSetIfChanged(ref _plannedQuantity, value);
    }

    private decimal? _assignedQuantity;
    public decimal? AssignedQuantity
    {
        get => _assignedQuantity;
        set => this.RaiseAndSetIfChanged(ref _assignedQuantity, value);
    }

    private decimal? _completedQuantity;
    public decimal? CompletedQuantity
    {
        get => _completedQuantity;
        set => this.RaiseAndSetIfChanged(ref _completedQuantity, value);
    }

    public string StatusText => CompletedQuantity >= PlannedQuantity ? "Готово" :
                                AssignedQuantity > 0 ? "В работе" : "Ожидает";

    public SolidColorBrush StatusColor => CompletedQuantity >= PlannedQuantity
        ? new SolidColorBrush(Colors.LimeGreen)
        : AssignedQuantity > 0
            ? new SolidColorBrush(Colors.Orange)
            : new SolidColorBrush(Colors.Gray);

    public bool CanAssign => AssignedQuantity < PlannedQuantity;

    public ICommand AssignCommand 
        => ReactiveCommand.Create(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseEmployeeAssignmentMasterSubMarkStatusMessage(true, OperationId)));

    public bool IsAdministrator
        => ManagerCookie.IsUserLoggedIn() && ManagerCookie.IsAdministrator;
}
