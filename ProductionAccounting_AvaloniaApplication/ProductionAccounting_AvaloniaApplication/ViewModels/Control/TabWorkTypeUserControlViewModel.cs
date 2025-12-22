using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class TabWorkTypeUserControlViewModel : ViewModelBase, IRecipient<RefreshWorkTypeListMessage>
{
    public TabWorkTypeUserControlViewModel() 
    {
        WeakReferenceMessenger.Default.Register(this);

        _ = LoadWorkTypeAsync();
    }

    public async void Receive(RefreshWorkTypeListMessage message) 
    {
        try
        {
            await LoadWorkTypeAsync();
        }
        catch (Exception ex) 
        {
            Loges.LoggingProcess(level: LogLevel.Critical,
                ex: ex);
        }
    }

    private ObservableCollection<CartTypeWork> _cartTypeWork = [];
    public ObservableCollection<CartTypeWork> CartTypeWork
    {
        get => _cartTypeWork;
        set => this.RaiseAndSetIfChanged(ref _cartTypeWork, value);
    }

    private async Task LoadWorkTypeAsync() 
    {
        try 
        {
            const string sql = "SELECT id, type, coefficient, measurement, rate FROM public.work_type";

            var tempItems = new List<CartTypeWork>();

            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand(sql, connection);
            await using var reader = await command.ExecuteReaderAsync();
            while (await reader.ReadAsync()) 
            {
                tempItems.Add(new CartTypeWork(id: reader.GetDouble(0), type: reader.GetString(1), coefficient: reader.GetDecimal(2), measurement: reader.GetString(3), rate: reader.GetDecimal(4)));
            }

            CartTypeWork = new ObservableCollection<CartTypeWork>(tempItems);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning, 
                ex: ex);
        }
    }
}
