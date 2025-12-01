using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class CartSubProductUserControlViewModel : ViewModelBase
{
    public ICommand DeleteCommand
        => new RelayCommand(async () => await DeleteAsync());

    public ICommand ViewCommand
        => new RelayCommand(async () => { WeakReferenceMessenger.Default.Send(new OpenOrCloseSubProductStatusMessage()); });

    private double? _id;
    public double? Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }

    private double? _productTaskId;
    public double? ProductTaskId
    {
        get => _productTaskId;
        set => this.RaiseAndSetIfChanged(ref _productTaskId, value);
    }

    private string? _name;
    public string? Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    private double? _plannedQuantity;
    public double? PlannedQuantity
    {
        get => _plannedQuantity;
        set => this.RaiseAndSetIfChanged(ref _plannedQuantity, value);
    }

    private double? _plannedWeight;
    public double? PlannedWeight
    {
        get => _plannedWeight;
        set => this.RaiseAndSetIfChanged(ref _plannedWeight, value);
    }

    private string? _notes;
    public string? Notes
    {
        get => _notes;
        set => this.RaiseAndSetIfChanged(ref _notes, value);
    }

    private DateTime? _createAt;
    public DateTime? CreateAt
    {
        get => _createAt;
        set => this.RaiseAndSetIfChanged(ref _createAt, value);
    }

    private List<CartOperationUserControl> _operations = [];
    public List<CartOperationUserControl> Operations 
    {
        get => _operations;
        set => this.RaiseAndSetIfChanged(ref _operations, value);
    }

    public bool IsNullNotes
        => !string.IsNullOrEmpty(Notes);

    public bool IsAdministrator
        => ManagerCookie.IsUserLoggedIn() && ManagerCookie.IsAdministrator;

    private async Task<bool> DeleteAsync()
    {
        try
        {
            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                try
                {
                    string sql1 = "DELETE FROM public.sub_products WHERE id = @id";
                    using (var command1 = new NpgsqlCommand(sql1, connection))
                    {
                        command1.Parameters.AddWithValue("@id", Id ?? 0);
                        await command1.ExecuteNonQueryAsync();
                    }

                    Loges.LoggingProcess(level: LogLevel.INFO,
                        message: $"Deleted position {Id}, Name {Name}");

                    WeakReferenceMessenger.Default.Send(new RefreshSubProductListMessage());

                    return true;
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
