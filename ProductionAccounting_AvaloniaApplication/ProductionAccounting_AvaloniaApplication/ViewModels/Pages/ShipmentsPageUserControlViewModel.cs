using Avalonia.Controls;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.ViewModels.Control;
using ProductionAccounting_AvaloniaApplication.Views.Control;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Pages;

public class ShipmentsPageUserControlViewModel : ViewModelBase
{
    public Grid? Content { get; set; } = null;
    public StackPanel? CartShipment { get; set; } = null;

    private List<CartShipmentUserControl> shipmentsList = [];

    public async Task LoadShipmentsAsync() 
    {
        try
        {
            var sql = @"
                    SELECT 
                        s.id, 
                        s.order_id, 
                        s.shipment_date, 
                        s.status, 
                        s.shipped_weight, 
                        s.shipped_quantity, 
                        s.notes, 
                        s.created_by, 
                        s.created_at,
                        o.order_number, 
                        o.customer_name
                FROM public.shipments s
                JOIN public.orders o ON o.id = s.order_id
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
                                Id = reader.GetDouble(0),
                                OrderId = reader.GetDouble(1),
                                ShipmentDate = reader.GetDateTime(2),
                                Status = reader.GetString(3),
                                ShipmentpedWeight = reader.GetDecimal(4),
                                ShippedQuantity = reader.GetDecimal(5),
                                Notes = reader.GetString(6),
                                CreateBy = reader.GetDouble(7),
                                CreateAt = reader.GetDateTime(8),
                                OrderNubmer = reader.GetString(9),
                                CustomerName = reader.GetString(10),
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
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);
        }
    }
}
