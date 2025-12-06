using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels;

public class ConfirmDeleteOperationWindowViewModel : ViewModelBase
{
    private ConfirmDeleteOperationWindow? window { get; set; } = null;

    public void SetWindow(ConfirmDeleteOperationWindow _window)
    {
        window = _window;
    }

    public ICommand CancelCommand
        => new RelayCommand(() => window?.Close());

    public ICommand DeleteCommand
        => new RelayCommand(async () => 
        { 
            if ( await DeleteAsync() ) 
            { 
                WeakReferenceMessenger.Default.Send(new RefreshOperationListMessage());
                window?.Close();
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

    private async Task<bool> DeleteAsync()
    {
        try
        {
            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                try
                {
                    var deleteQueries = new[]
{
                        "DELETE FROM public.production WHERE product_id = @id"
                    };

                    foreach (var querty in deleteQueries)
                    {
                        await using (var command = new NpgsqlCommand(querty, connection))
                        {
                            command.Parameters.AddWithValue("@id", Id);
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    using (var command1 = new NpgsqlCommand("DELETE FROM public.operation WHERE id = @id", connection))
                    {
                        command1.Parameters.AddWithValue("@id", Id);
                        await command1.ExecuteNonQueryAsync();
                    }

                    Loges.LoggingProcess(level: LogLevel.INFO,
                        message: $"Deleted user {Id}, Name {Name}");

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
