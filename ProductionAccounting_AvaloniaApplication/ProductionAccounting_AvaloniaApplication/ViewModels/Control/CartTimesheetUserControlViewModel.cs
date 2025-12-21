using ReactiveUI;
using System;
using JetBrains.Annotations;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartTimesheetUserControlViewModel : ViewModelBase
{
    private string _login = string.Empty;
    public string Login 
    {
        get => _login;
        set => this.RaiseAndSetIfChanged(ref _login, value);
    }

    private string _status = string.Empty;
    public string Status 
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    private string _notes = string.Empty;
    public string Notes
    {
        get => _notes;
        set => this.RaiseAndSetIfChanged(ref _notes, value);
    }

    private decimal _hoursWorked;
    public decimal HoursWorked
    {
        get => _hoursWorked;
        set => this.RaiseAndSetIfChanged(ref _hoursWorked, value);
    }

    private DateTimeOffset? _workDate;
    [UsedImplicitly]
    public DateTimeOffset? WorkDate
    {
        get => _workDate;
        set => this.RaiseAndSetIfChanged(ref _workDate, value);
    }

    public bool IsVisibleWorkDate 
        => WorkDate != null;
}
