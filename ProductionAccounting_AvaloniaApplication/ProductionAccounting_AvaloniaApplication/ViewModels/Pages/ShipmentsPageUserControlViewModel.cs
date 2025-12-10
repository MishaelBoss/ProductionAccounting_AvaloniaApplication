using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class ShipmentsPageUserControlViewModel : ViewModelBase, IRecipient<RefreshShipmentListMessage>
{
    public ShipmentsPageUserControlViewModel()
    {
        WeakReferenceMessenger.Default.Register(this);
    }

    public void Receive(RefreshShipmentListMessage message)
    {
        _ = LoadShipmentsAsync();
    }

    public StackPanel? CartShipment { get; set; } = null;

    private List<CartShipmentUserControl> shipmentsList = [];

    public ICommand RefreshCommand
        => new RelayCommand(async () => await LoadShipmentsAsync());

    public async Task LoadShipmentsAsync() 
    {
        StackPanelHelper.ClearAndRefreshStackPanel<CartShipmentUserControl>(CartShipment, shipmentsList);

        try
        {
            string sql = @"
                    SELECT 
                        s.id,
                        s.product_task_id,
                        pt.product_id,
                        p.name,
                        p.article,
                        s.planned_quantity,
                        s.shipped_quantity,
                        s.shipped_weight,
                        s.shipment_date,
                        s.status,
                        s.notes,
                        s.created_by,
                        s.created_at
                    FROM public.shipments s
                    JOIN public.product_tasks pt ON pt.id = s.product_task_id
                    JOIN public.product p ON p.id = pt.product_id
                    ORDER BY s.created_at DESC";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    using (var reader = await command.ExecuteReaderAsync()) {
                        while (await reader.ReadAsync())
                        {
                            var viewModel = new CartShipmentUserControlViewModel()
                            {
                                Id = reader.IsDBNull(0) ? 0 : reader.GetDouble(0),
                                ProductTaskId = reader.IsDBNull(1) ? 0 : reader.GetDouble(1),
                                ProductId = reader.IsDBNull(2) ? 0 : reader.GetDouble(2),
                                ProductName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3),
                                Article = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                PlannedQuantity = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5),
                                ShippedQuantity = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6),
                                ShipmentpedWeight = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7),
                                ShipmentDate = reader.IsDBNull(8) ? DateTime.UtcNow : reader.GetFieldValue<DateTime>(8),
                                Status = reader.IsDBNull(9) ? string.Empty : reader.GetString(9),
                                Notes = reader.IsDBNull(10) ? string.Empty : reader.GetString(10),
                                CreateAt = reader.IsDBNull(12) ? DateTime.UtcNow : reader.GetFieldValue<DateTime>(12),
                            };

                            var userControl = new CartShipmentUserControl()
                            { 
                                DataContext = viewModel,
                            };

                            shipmentsList.Add(userControl);
                        }
                    }
                }
            }

            StackPanelHelper.RefreshStackPanelContent<CartShipmentUserControl>(CartShipment, shipmentsList);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);
        }
    }
}
