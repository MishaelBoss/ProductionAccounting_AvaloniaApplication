using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartEmployeeTaskUserControlViewModel : ViewModelBase
{
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
        => Status != "completed" && Status != "rejected";

    public bool HasNotes 
        => !string.IsNullOrWhiteSpace(Notes);

    public ICommand ChangeStatusCommand
        => new RelayCommand(async () => await ChangeStatusAsync());

    private async Task ChangeStatusAsync()
    {
        var newStatus = Status == "completed" ? "assigned" : "completed";

        try
        {
            using var conn = new NpgsqlConnection(Arguments.connection);
            await conn.OpenAsync();

            string sql = "UPDATE public.task_assignments SET status = @status WHERE id = @id";
            using var cmd = new NpgsqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@status", newStatus);
            cmd.Parameters.AddWithValue("@id", AssignmentId);

            await cmd.ExecuteNonQueryAsync();

            Status = newStatus;

            //WeakReferenceMessenger.Default.Send(new RefreshEmployeeTasksMessage());
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.ERROR, ex: ex);
        }
    }
}
