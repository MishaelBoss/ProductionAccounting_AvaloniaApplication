using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class AddShipmentUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{
    public ICommand CancelCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseStatusMessage(false)));

    public ICommand ConfirmCommand
        => new RelayCommand(async () => 
        {
            if (await SaveAsync()) 
            { 
                WeakReferenceMessenger.Default.Send(new OpenOrCloseStatusMessage(false)); 
                WeakReferenceMessenger.Default.Send(new RefreshShipmentListMessage()); 
            } 
        });

    private ObservableCollection<ComboBoxOrderForShipment> _comboBoxOrders = [];
    public ObservableCollection<ComboBoxOrderForShipment> ComboBoxOrders
    {
        get => _comboBoxOrders;
        set => this.RaiseAndSetIfChanged(ref _comboBoxOrders, value);
    }

    private ComboBoxOrderForShipment? _selectedOrder;
    public ComboBoxOrderForShipment? SelectedOrder
    {
        get => _selectedOrder;
        set
        {
            if (_selectedOrder != value)
            {
                _selectedOrder = value;
                OnPropertyChanged(nameof(SelectedOrder));
                OnPropertyChanged(nameof(IsActiveConfirmButton));
                OnPropertyChanged(nameof(ShipmentInfo));
            }
        }
    }

    private string _notes = string.Empty;
    public string Notes
    {
        get => _notes;
        set
        {
            if (_notes != value)
            {
                _notes = value;
                OnPropertyChanged(nameof(Notes));
            }
        }
    }

    private string _messageerror = string.Empty;
    public string Messageerror
    {
        get => _messageerror;
        set => this.RaiseAndSetIfChanged(ref _messageerror, value);
    }

    public bool IsActiveConfirmButton => SelectedOrder != null;

    public string ShipmentInfo => SelectedOrder != null
        ? $"Заказ: {SelectedOrder.OrderNumber}\n" +
          $"Контрагент: {SelectedOrder.CustomerName}\n" +
          $"Готово к отгрузке: {SelectedOrder.ProducedWeight:F3} т / {SelectedOrder.ProducedQuantity:F2} шт."
        : "Выберите заказ из списка";

    public async Task LoadOrdersAsync()
    {
        try
        {
            string sql = @"
                    SELECT 
                        o.id, 
                        o.order_number, 
                        o.customer_name,
                        COALESCE(SUM(oi.planned_weight), 0) as planned_weight,
                        COALESCE(SUM(p.quantity), 0) as produced_weight,
                        COALESCE(SUM(oi.planned_quantity), 0) as planned_qty,
                        COALESCE(SUM(p.quantity), 0) as produced_qty
                    FROM public.orders o
                    LEFT JOIN public.order_items oi ON oi.order_id = o.id
                    LEFT JOIN public.production p ON p.product_id = oi.product_id AND p.status = 'completed'
                    GROUP BY o.id, o.order_number, o.customer_name
                    HAVING COALESCE(SUM(p.quantity), 0) > 0
                    ORDER BY o.order_number";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            ComboBoxOrders.Add(new ComboBoxOrderForShipment(
                                reader.GetDouble(0),
                                reader.GetString(1),
                                reader.GetString(2),
                                reader.GetDecimal(4),
                                reader.GetDecimal(6)
                            ));
                        }
                    }   
                }
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.ERROR, 
                ex: ex);
            Messageerror = "Ошибка загрузки заказов";
        }
    }

    public async Task<bool> SaveAsync()
    {
        if (SelectedOrder == null)
        {
            Messageerror = "Выберите заказ";
            return false;
        }

        try
        {
            string sql = @"
                INSERT INTO public.shipments 
                (order_id, shipment_date, status, shipped_weight, shipped_quantity, notes, created_by)
                VALUES (@order_id, CURRENT_DATE, 'formed', @weight, @quantity, @notes, @created_by)";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@order_id", SelectedOrder.Id);
                    command.Parameters.AddWithValue("@weight", SelectedOrder.ProducedWeight);
                    command.Parameters.AddWithValue("@quantity", SelectedOrder.ProducedQuantity);
                    command.Parameters.AddWithValue("@notes", Notes ?? "");
                    command.Parameters.AddWithValue("@created_by", ManagerCookie.GetIdUser ?? 0);

                    await command.ExecuteNonQueryAsync();
                }
            }

            Messageerror = "Отгрузка успешно создана!";
            return true;
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.ERROR, 
                ex: ex);
            Messageerror = "Ошибка при создании отгрузки";
            return false;
        }
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged(string propertyName)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
