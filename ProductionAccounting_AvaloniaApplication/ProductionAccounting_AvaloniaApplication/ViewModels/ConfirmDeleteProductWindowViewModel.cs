using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ProductionAccounting_AvaloniaApplication.Views;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels;

public class ConfirmDeleteProductWindowViewModel : ViewModelBase
{
    private ConfirmDeleteProductWindow? window { get; set; } = null;

    public void SetWindow(ConfirmDeleteProductWindow _window)
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
                WeakReferenceMessenger.Default.Send(new RefreshProductListMessage());
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
                    string sql1 = "DELETE FROM public.product WHERE id = @id";
                    using (var command1 = new NpgsqlCommand(sql1, connection))
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
