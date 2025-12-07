using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels;

public class ViewModelBase : ReactiveObject
{
    private bool _isAdministrator;
    public bool IsAdministrator
    {
        get => _isAdministrator;
        set => this.RaiseAndSetIfChanged(ref _isAdministrator, value);
    }

    private bool _isMaster;
    public bool IsMaster
    {
        get => _isMaster;
        set => this.RaiseAndSetIfChanged(ref _isMaster, value);
    }

    private bool _isEmployee;
    public bool IsEmployee
    {
        get => _isEmployee;
        set => this.RaiseAndSetIfChanged(ref _isEmployee, value);
    }

    private bool _isManager;
    public bool IsManager
    {
        get => _isManager;
        set => this.RaiseAndSetIfChanged(ref _isManager, value);
    }

    private bool _isAdministratorOrMaster;
    public bool IsAdministratorOrMaster
    {
        get => _isAdministratorOrMaster;
        set => this.RaiseAndSetIfChanged(ref _isAdministratorOrMaster, value);
    }

    private bool _isAdministratorOrManager;
    public bool IsAdministratorOrManager
    {
        get => _isAdministratorOrManager;
        set => this.RaiseAndSetIfChanged(ref _isAdministratorOrManager, value);
    }

    private bool _isAdministratorOrMasterOrEmployee;
    public bool IsAdministratorOrMasterOrEmployee
    {
        get => _isAdministratorOrMasterOrEmployee;
        set => this.RaiseAndSetIfChanged(ref _isAdministratorOrMasterOrEmployee, value);
    }

    private bool _isAdministratorOrMasterOrManager;
    public bool IsAdministratorOrMasterOrManager
    {
        get => _isAdministratorOrMasterOrManager;
        set => this.RaiseAndSetIfChanged(ref _isAdministratorOrMasterOrManager, value);
    }

    private bool _isAll;
    public bool IsAll
    {
        get => _isAll;
        set => this.RaiseAndSetIfChanged(ref _isAll, value);
    }
}
