using System;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.Views;
using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels;

public class ConfirmDeleteShipmentWindowViewModel : ViewModelBase
{
    private ConfirmDeleteShipmentWindow? window { get; set; } = null;

    public void SetWindow(ConfirmDeleteShipmentWindow _window)
    {
        window = _window;
    }

    public ICommand CancelCommand
        => new RelayCommand(() => window?.Close());

    public ICommand DeleteCommand
        => new RelayCommand(async () => await DeleteAsync());

    private double _id = 0;
    public double Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }

    private string _name = string.Empty;
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    private string _orderNumber = string.Empty;
    public string OrderNumber
    {
        get => _orderNumber;
        set => this.RaiseAndSetIfChanged(ref _orderNumber, value);
    }

    private DateTime _shipmentDate;
    public DateTime ShipmentDate
    {
        get => _shipmentDate;
        set => this.RaiseAndSetIfChanged(ref _shipmentDate, value);
    }

    private async Task DeleteAsync()
    {
        if(!IsAdministrator) return;

        try
        {
            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                try
                {
                    using (var command = new NpgsqlCommand("DELETE FROM public.shipments WHERE id = @id", connection))
                    {
                        command.Parameters.AddWithValue("@id", Id);
                        var rowsAffected = await command.ExecuteNonQueryAsync();

                        if(rowsAffected > 0) 
                        {
                            Loges.LoggingProcess(level: LogLevel.INFO,
                                message: $"Deleted user {Id}, Name {Name}");

                            WeakReferenceMessenger.Default.Send(new RefreshShipmentListMessage());
                            window?.Close();
                        }
                    }
                }
                catch (Exception ex)
                {
                    Loges.LoggingProcess(level: LogLevel.ERROR,
                        ex: ex);
                }
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.ERROR,
                ex: ex);
        }
    }
}
