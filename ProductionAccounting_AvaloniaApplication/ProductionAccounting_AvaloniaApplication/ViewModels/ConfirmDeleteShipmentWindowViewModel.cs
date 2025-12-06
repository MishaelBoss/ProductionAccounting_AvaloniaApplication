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
    private ConfirmDeleteShipmentWindow? shipmentWindow { get; set; } = null;

    public void SetWindow(ConfirmDeleteShipmentWindow window)
    {
        shipmentWindow = window;
    }

    public ICommand CancelCommand
        => new RelayCommand(() => shipmentWindow?.Close());

    public ICommand DeleteCommand
        => new RelayCommand(async () => 
        { 
            if ( await DeleteAsync() ) 
            { 
                WeakReferenceMessenger.Default.Send(new RefreshShipmentListMessage());
                shipmentWindow?.Close();
            } 
        });

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

    public bool IsAdministrator
        => ManagerCookie.IsUserLoggedIn() && ManagerCookie.IsAdministrator;

    private async Task<bool> DeleteAsync()
    {
        if(!IsAdministrator) return false;

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
                        var rows = await command.ExecuteNonQueryAsync();

                        if(rows > 0) 
                        {
                            Loges.LoggingProcess(level: LogLevel.INFO,
                                message: $"Deleted user {Id}, Name {Name}");
                                
                            return true;
                        }
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    Loges.LoggingProcess(level: LogLevel.ERROR,
                        ex: ex);
                    return false;
                }
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.ERROR,
                ex: ex);
            return false;
        }
    }
}
