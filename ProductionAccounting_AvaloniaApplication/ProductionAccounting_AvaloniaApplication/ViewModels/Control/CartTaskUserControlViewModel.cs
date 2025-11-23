using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartTaskUserControlViewModel : ViewModelBase
{
    private bool _isWork;
    public bool IsWork
    {
        get => _isWork;
        set => this.RaiseAndSetIfChanged(ref _isWork, value);
    }

    private double _productionId;
    public double ProductionId
    {
        get => _productionId;
        set => this.RaiseAndSetIfChanged(ref _productionId, value);
    }

    private string _productName = string.Empty;
    public string ProductName 
    {
        get => _productName;
        set => this.RaiseAndSetIfChanged(ref _productName, value);
    }

    private string _operationName = string.Empty;
    public string OperationName
    {
        get => _operationName;
        set => this.RaiseAndSetIfChanged(ref _operationName, value);
    }

    private string _unit = string.Empty;
    public string Unit
    {
        get => _unit;
        set => this.RaiseAndSetIfChanged(ref _unit, value);
    }

    private string _notes = string.Empty;
    public string Notes
    {
        get => _notes;
        set => this.RaiseAndSetIfChanged(ref _notes, value);
    }

    private decimal _quantity;
    public decimal Quantity
    {
        get => _quantity;
        set => this.RaiseAndSetIfChanged(ref _quantity, value);
    }

    private decimal _amount;
    public decimal Amount
    {
        get => _amount;
        set => this.RaiseAndSetIfChanged(ref _amount, value);
    }

    private DateTimeOffset _createdAt;
    public DateTimeOffset CreatedAt
    {
        get => _createdAt;
        set => this.RaiseAndSetIfChanged(ref _createdAt, value);
    }

    private string _status = "issued";
    public string Status
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    public string StatusDisplay => Status switch
    {
        "issued" => "Ожидает подтверждения",
        "completed" => "Сдано",
        "rejected" => "Отменено",
        _ => "—"
    };

    public Brush ButtonColor => Status switch
    {
        "issued" => new SolidColorBrush(Colors.Orange),
        "completed" => new SolidColorBrush(Colors.LimeGreen),
        "rejected" => new SolidColorBrush(Colors.Crimson),
        _ => new SolidColorBrush(Colors.Gray)
    };

    public bool HasNotes 
        => !string.IsNullOrWhiteSpace(Notes);

    public string ButtonText => Status == "completed" ? "Переоткрыть" : "Сдать работу";

    public bool CanConfirm => Status != "completed";

    public ICommand ChangeStatusCommand
        => new RelayCommand(async () => await ChangeStatusAsync());

    private async Task ChangeStatusAsync()
    {
        var newStatus = Status == "completed" ? "issued" : "completed";

        try
        {
            var sql = "UPDATE public.production SET status = @status WHERE id = @id";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@status", newStatus);
                    command.Parameters.AddWithValue("@id", ProductionId);

                    await command.ExecuteNonQueryAsync();

                    Status = newStatus;
                    OnStatusChanged();
                }
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);
        }
    }

    private void OnStatusChanged()
    {
        this.RaisePropertyChanged(nameof(StatusDisplay));
        this.RaisePropertyChanged(nameof(ButtonColor));
        this.RaisePropertyChanged(nameof(ButtonText));
        this.RaisePropertyChanged(nameof(CanConfirm));
    }
}
