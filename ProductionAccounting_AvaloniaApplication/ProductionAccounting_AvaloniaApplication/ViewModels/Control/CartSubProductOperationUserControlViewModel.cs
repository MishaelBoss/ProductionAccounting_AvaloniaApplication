using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MsBox.Avalonia;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartSubProductOperationUserControlViewModel : ViewModelBase
{
    public double AssignmentId { get; set; }
    public double SubProductOperationId { get; set; }
    public double ProductId { get; set; }
    public double OperationId { get; set; }
    public double SubProductId { get; set; }

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

    public bool IsAdministratorOrMaster
        => ManagerCookie.IsUserLoggedIn() && ManagerCookie.IsAdministrator || ManagerCookie.IsMaster;

    public bool CanAssignTest
        => IsAdministratorOrMaster 
        && IsAssigned 
        && !IsAssignedToMe
        && !IsCompleted;

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
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseEditSubOperationStatusMessage(true, SubProductOperationId)));

    public ICommand DeleteCommand 
        => new RelayCommand(async () => await DeleteSubOperationAsync());

    public ICommand AssignCommand 
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseEmployeeAssignmentMasterSubMarkStatusMessage(true, SubProductOperationId)));

    public ICommand OpenSubmitWorkForAnEmployeeCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseCompleteWorkFormStatusMessage(true, OperationName, PlannedQuantity, ProductId, OperationId, SubProductOperationId)));

    private async Task DeleteSubOperationAsync()
    {
        if(!Internet.ConnectToDataBase()) return;

        try
        {
            var sql = "DELETE FROM public.sub_product_operations WHERE id = @id";
            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    int rowsAffected = 0;

                    command.Parameters.AddWithValue("@id", SubProductOperationId);
                    rowsAffected = await command.ExecuteNonQueryAsync();

                    if(rowsAffected > 0)
                    {
                        Loges.LoggingProcess(level: LogLevel.INFO,
                            message: $"Deleted sub operation {SubProductOperationId}, Type {OperationName}");

                        WeakReferenceMessenger.Default.Send(new RefreshSubProductOperationsMessage(SubProductId));
                    }
                }
            }
        }
        catch (PostgresException ex)
        {
            Loges.LoggingProcess(LogLevel.ERROR,
                "Connection or request error",
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.ERROR, 
                ex: ex);
        }
    }
}
