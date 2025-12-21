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
    public double Id { get; set; }
    public double ProductTaskId { get; set; }
    public double ProductId { get; set; }

    public string ProductName { get; set; } = string.Empty;
    public string Article { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    public string CreateByName { get; set; } = string.Empty;

    public decimal? ShipmentpedWeight { get; set; }
    public decimal? ShippedQuantity { get; set; }
    public decimal? PlannedQuantity { get; set; }

    public DateTime ShipmentDate { get; set; }
    public DateTime CreateAt { get; set; }

    public ICommand ChangeStatusCommand
        => new RelayCommand(async () => await ChangeStatus());

    public ICommand DeleteCommand
        => new RelayCommand(() =>
        {
            var viewModel = new ConfirmDeleteWindowViewModel(Id, ProductName ?? "Заказ без названия", "DELETE FROM public.shipments WHERE id = @id", () => WeakReferenceMessenger.Default.Send(new RefreshShipmentListMessage()));

            var window = new ConfirmDeleteWindow()
            {
                DataContext = viewModel,
            };

            viewModel.SetWindow(window);

            window.Show();
        });

    public string StatusDisplay => Status switch
    {
        "formed" => "Сформирована",
        "shipped" => "Отгружена",
        "delivered" => "доставлено",
        _ => "Неизвестно"
    };

    public Brush ButtonColor => Status switch
    {
        "formed" => new SolidColorBrush(Color.Parse("#FFA500")),
        "shipped" => new SolidColorBrush(Color.Parse("#007AFF")),
        "delivered" => new SolidColorBrush(Color.Parse("#34C759")),
        _ => new SolidColorBrush(Colors.Gray)
    };

    public static bool IsAdministrator
        => ManagerCookie.IsUserLoggedIn() 
        && ManagerCookie.IsAdministrator;

    public bool CanNotes 
        => !string.IsNullOrEmpty(Notes);

    private async Task ChangeStatus() 
    {
        if(!IsAdministrator) return;

        var newStatus = Status == "formed" ? "shipped" : "delivered";

        try
        {
            var sql = @"UPDATE public.shipments SET status = @newStatus WHERE id = @id";

            using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();
            using var command = new NpgsqlCommand(sql, connection);
            command.Parameters.AddWithValue("@newStatus", newStatus);
            command.Parameters.AddWithValue("@id", Id);
            await command.ExecuteNonQueryAsync();

            Status = newStatus;
            OnStatusChanged();

            WeakReferenceMessenger.Default.Send(new RefreshShipmentListMessage());
        }
        catch (Exception ex) 
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
    }

    private void OnStatusChanged()
    {
        this.RaisePropertyChanged(nameof(StatusDisplay));
        this.RaisePropertyChanged(nameof(ButtonColor));
    }
}
