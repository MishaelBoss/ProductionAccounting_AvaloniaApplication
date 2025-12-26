using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartSubProductOperationUserControlViewModel : ViewModelBase
{
    public double AssignmentId { get; set; }
    public double SubProductOperationId { get; set; }
    public double ProductId { get; set; }
    public double OperationId { get; set; }
    public double SubProductId { get; set; }
    public double UserId { get; set; }

    public string Status { get; set; } = string.Empty;
    public string OperationCode { get; set; } = string.Empty;

    private string _operationName = string.Empty;
    public string OperationName
    {
        get => _operationName;
        set => this.RaiseAndSetIfChanged(ref _operationName, value);
    }

    private string _userName = string.Empty;
    public string UserName
    {
        get => _userName;
        set => this.RaiseAndSetIfChanged(ref _userName, value);
    }

    public string OperationDescription { get; set; } = string.Empty;

    public decimal OperationPrice { get; set; }
    public decimal OperationTime { get; set; }

    private decimal _plannedQuantity;
    public decimal PlannedQuantity
    {
        get => _plannedQuantity;
        set
        {
            if (_plannedQuantity != value)
            {
                _plannedQuantity = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(IsCompleted));
                this.RaisePropertyChanged(nameof(CanAssign));
            }
        }
    }

    private decimal _completedQuantity;
    public decimal CompletedQuantity
    {
        get => _completedQuantity;
        set
        {
            if (_completedQuantity != value)
            {
                _completedQuantity = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(IsCompleted));
                this.RaisePropertyChanged(nameof(StatusText));
                this.RaisePropertyChanged(nameof(StatusColor));
                this.RaisePropertyChanged(nameof(CanAssign));
            }
        }
    }

    private double? _assignedToUserId;
    public double? AssignedToUserId
    {
        get => _assignedToUserId;
        set
        {
            if (_assignedToUserId != value)
            {
                _assignedToUserId = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(IsAssigned));
                this.RaisePropertyChanged(nameof(IsAssignedToMe));
                this.RaisePropertyChanged(nameof(CanAssign));
                this.RaisePropertyChanged(nameof(CanSubmit));
                this.RaisePropertyChanged(nameof(StatusText));
                this.RaisePropertyChanged(nameof(StatusColor));
            }
        }
    }

    public bool IsAssigned 
        => AssignedToUserId.HasValue;

    public bool IsAssignedToMe 
        => AssignedToUserId == ManagerCookie.GetIdUser;

    public bool CanSubmit 
        => IsAssignedToMe 
        && !IsCompleted;
    public bool IsCompleted
        => CompletedQuantity >= PlannedQuantity;

    public bool CanAssign 
        => !IsAssigned 
        && !IsCompleted;

    public static bool IsAdministratorOrMasterAndManager
        => ManagerCookie.IsUserLoggedIn();

    public bool CanAssignTest
        => IsAdministratorOrMasterAndManager 
        && IsAssigned 
        && !IsAssignedToMe
        && !IsCompleted;

    public bool IsAdministratorOrMasterAndCanCompleteTask
        => IsAdministratorOrMasterAndManager 
        && Status != "completed";

    public string StatusButtonText 
        => IsAssigned ? "Переназначить" 
        : "Назначить";

    public string StatusText 
        => IsCompleted ? "Готово" 
        : IsAssignedToMe ? "Ваша задача" 
        : IsAssigned ? "Занято" 
        : "Ожидает";

    public SolidColorBrush StatusColor 
        => IsCompleted ? new(Colors.LimeGreen)
        : IsAssignedToMe ? new(Colors.Orange)
        : IsAssigned ? new(Colors.Gray)
        : new(Colors.Cyan);

    public ICommand EditCommand 
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseSubOperationStatusMessage(true, SubProductId, SubProductOperationId, OperationName, OperationCode, OperationPrice, OperationTime, OperationDescription, PlannedQuantity)));

/*    public ICommand DeleteCommand
        => new RelayCommand(() =>
        {
            var viewModel = new ConfirmDeleteWindowViewModel(SubProductOperationId, OperationName ?? string.Empty, "DELETE FROM public.sub_product_operations WHERE id = @id", (() => WeakReferenceMessenger.Default.Send(new RefreshSubProductOperationsMessage(SubProductId))));

            var window = new ConfirmDeleteWindow()
            {
                DataContext = viewModel,
            };

            viewModel.SetWindow(window);

            window.Show();
        });*/

/*    public ICommand AssignCommand 
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseEmployeeAssignmentMasterSubMarkStatusMessage(true, SubProductOperationId, SubProductId)));*/
    //
    // public ICommand OpenSubmitWorkForAnEmployeeCommand
    //     => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseCompleteWorkFormStatusMessage(true, OperationName, PlannedQuantity, ProductId, OperationId, SubProductOperationId, AssignedToUserId)));
}
