using ReactiveUI;
using System;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartTaskAssignmentUserControlViewModel : ViewModelBase
{
    private double? _id;
    public double? Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }

    private double? _subProductOperationId;
    public double? SubProductOperationId
    {
        get => _subProductOperationId;
        set => this.RaiseAndSetIfChanged(ref _subProductOperationId, value);
    }

    private double? _userId;
    public double? UserId
    {
        get => _userId;
        set => this.RaiseAndSetIfChanged(ref _userId, value);
    }

    private decimal? _assignedQuantity;
    public decimal? AssignedQuantity
    {
        get => _assignedQuantity;
        set => this.RaiseAndSetIfChanged(ref _assignedQuantity, value);
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

    private double? _assignedBy;
    public double? AssignedBy
    {
        get => _assignedBy;
        set => this.RaiseAndSetIfChanged(ref _assignedBy, value);
    }

    private DateTime? _assignedAt;
    public DateTime? AssignedAt
    {
        get => _assignedAt;
        set => this.RaiseAndSetIfChanged(ref _assignedAt, value);
    }


    private string? _employeeName;
    public string? EmployeeName
    {
        get => _employeeName;
        set => this.RaiseAndSetIfChanged(ref _employeeName, value);
    }

    public string? StatusDisplay => Status switch
    {
        "assigned" => "Назначено",
        "in_progress" => "В работе",
        "confirmed" => "Подтверждено",
        _ => Status
    };
}
