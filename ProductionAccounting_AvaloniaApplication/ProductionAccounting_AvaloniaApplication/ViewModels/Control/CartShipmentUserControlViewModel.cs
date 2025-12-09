using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartShipmentUserControlViewModel : ViewModelBase
{

    public ICommand ChangeStatusCommand
        => new RelayCommand(async () => await ChangeStatus());

    public ICommand DeleteCommand
        => new RelayCommand(() =>
        {
            var viewModel = new ConfirmDeleteWindowViewModel(Id, CustomerName ?? "Заказ без названия", "DELETE FROM public.shipments WHERE id = @id", (() => WeakReferenceMessenger.Default.Send(new RefreshShipmentListMessage())));

            var window = new ConfirmDeleteWindow()
            {
                DataContext = viewModel,
            };

            viewModel.SetWindow(window);

            window.Show();
        });

    private double _id;
    public double Id 
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }

    private double _orderId;
    public double OrderId
    {
        get => _orderId;
        set => this.RaiseAndSetIfChanged(ref _orderId, value);
    }

    private DateTime _shipmentDate;
    public DateTime ShipmentDate 
    {
        get => _shipmentDate;
        set => this.RaiseAndSetIfChanged(ref _shipmentDate, value);
    }

    private string? _status;
    public string? Status 
    {
        get => _status;
        set => this.RaiseAndSetIfChanged(ref _status, value);
    }

    private decimal? _shippedWeight;
    public decimal? ShipmentpedWeight 
    {
        get => _shippedWeight;
        set => this.RaiseAndSetIfChanged(ref _shippedWeight, value);
    }

    private decimal? _shippedQuantity;
    public decimal? ShippedQuantity
    {
        get => _shippedQuantity;
        set => this.RaiseAndSetIfChanged(ref _shippedQuantity, value);
    }

    private string? _notes;
    public string? Notes
    {
        get => _notes;
        set => this.RaiseAndSetIfChanged(ref _notes, value);
    }

    private double _createBy;
    public double CreateBy
    {
        get => _createBy;
        set => this.RaiseAndSetIfChanged(ref _createBy, value);
    }

    private DateTime _createAt;
    public DateTime CreateAt
    {
        get => _createAt;
        set => this.RaiseAndSetIfChanged(ref _createAt, value);
    }

    private string? _orderNubmer;
    public string? OrderNubmer
    {
        get => _orderNubmer;
        set => this.RaiseAndSetIfChanged(ref _orderNubmer, value);
    }

    private string? _customerName;
    public string? CustomerName
    {
        get => _customerName;
        set => this.RaiseAndSetIfChanged(ref _customerName, value);
    }

    public string StatusDisplay => Status switch
    {
        "formed" => "Сформирована",
        "confirmed" => "Подтверждена",
        "shipped" => "Отгружена",
        _ => "Неизвестно"
    };

    public Brush ButtonColor => Status switch
    {
        "formed" => new SolidColorBrush(Color.Parse("#FFA500")),
        "confirmed" => new SolidColorBrush(Color.Parse("#007AFF")),
        "shipped" => new SolidColorBrush(Color.Parse("#34C759")),
        _ => new SolidColorBrush(Colors.Gray)
    };

    public bool IsAdministrator
        => ManagerCookie.IsUserLoggedIn() && ManagerCookie.IsAdministrator;

    private async Task ChangeStatus() 
    {
        if(!IsAdministrator) return;

        var newStatus = Status == "formed" ? "confirmed" : "shipped";

        try
        {
            var sql = @"UPDATE public.shipments SET status = @newStatus WHERE id = @id";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@newStatus", newStatus);
                    command.Parameters.AddWithValue("@id", Id);
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
    }
}
