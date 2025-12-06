using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartEmployeeTaskUserControlViewModel : ViewModelBase
{
    public double ProductId { get; set; }
    public double OperationId { get; set; }
    public double SubProductOperationId { get; set; }

    private double _assignmentId;
    public double AssignmentId
    {
        get => _assignmentId;
        set => this.RaiseAndSetIfChanged(ref _assignmentId, value);
    }

    private string _subProductName = "";
    public string SubProductName
    {
        get => _subProductName;
        set => this.RaiseAndSetIfChanged(ref _subProductName, value);
    }

    private string _operationName = "";
    public string OperationName
    {
        get => _operationName;
        set => this.RaiseAndSetIfChanged(ref _operationName, value);
    }

    private decimal _assignedQuantity;
    public decimal AssignedQuantity
    {
        get => _assignedQuantity;
        set => this.RaiseAndSetIfChanged(ref _assignedQuantity, value);
    }

    private decimal _completedQuantity;
    public decimal CompletedQuantity
    {
        get => _completedQuantity;
        set
        {
            this.RaiseAndSetIfChanged(ref _completedQuantity, value);
            this.RaisePropertyChanged(nameof(ProgressText));
            this.RaisePropertyChanged(nameof(IsCompleted));
            this.RaisePropertyChanged(nameof(CanComplete));
        }
    }

    private string? _notes;
    public string? Notes
    {
        get => _notes;
        set => this.RaiseAndSetIfChanged(ref _notes, value);
    }

    private string _status = "assigned";
    public string Status
    {
        get => _status;
        set
        {
            if (_status != value)
            {
                this.RaisePropertyChanged(nameof(StatusDisplay));
                this.RaisePropertyChanged(nameof(ButtonText));
                this.RaisePropertyChanged(nameof(ButtonColor));
                this.RaisePropertyChanged(nameof(CanComplete));
            }
        }
    }

    public string StatusDisplay => Status switch
    {
        "assigned" => "Назначено",
        "completed" => "Сдано",
        "rejected" => "Отменено",
        _ => "—"
    };

    public Brush ButtonColor => Status switch
    {
        "assigned" => new SolidColorBrush(Colors.Orange),
        "completed" => new SolidColorBrush(Colors.LimeGreen),
        "rejected" => new SolidColorBrush(Colors.Crimson),
        _ => new SolidColorBrush(Colors.Gray)
    };

    public string ButtonText 
        => Status == "completed" ? "Переоткрыть" : "Сдать работу";

    public bool CanComplete 
        => !IsCompleted;

    public string ProgressText => $"{CompletedQuantity:F0} / {AssignedQuantity:F0}";

    public bool IsCompleted => CompletedQuantity >= AssignedQuantity;

    public bool HasNotes 
        => !string.IsNullOrWhiteSpace(Notes);

    public ICommand OpenCompleteWorkFormCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseCompleteWorkFormStatusMessage(true, AssignmentId, OperationName, AssignedQuantity, ProductId, OperationId, SubProductOperationId)));
}
