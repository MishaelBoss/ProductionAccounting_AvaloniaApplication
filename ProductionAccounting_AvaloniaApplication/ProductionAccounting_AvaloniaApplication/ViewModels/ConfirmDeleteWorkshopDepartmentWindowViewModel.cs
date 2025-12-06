using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using MsBox.Avalonia;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels;

public class ConfirmDeleteWorkshopDepartmentWindowViewModel : ViewModelBase
{
    private ConfirmDeleteWorkshopDepartmentWindow? window { get; set; } = null;

    public void SetWindow(ConfirmDeleteWorkshopDepartmentWindow _window)
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
                WeakReferenceMessenger.Default.Send(new RefreshDepartmentListMessage());
                window?.Close();
            } 
        });

    private double _id = 0;
    public double Id
    {
        get => _id;
        set => this.RaiseAndSetIfChanged(ref _id, value);
    }

    private string _type = string.Empty;
    public string Type
    {
        get => _type;
        set => this.RaiseAndSetIfChanged(ref _type, value);
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
                    using (var command1 = new NpgsqlCommand("DELETE FROM public.departments WHERE id = @id", connection))
                    {
                        command1.Parameters.AddWithValue("@id", Id);
                        await command1.ExecuteNonQueryAsync();
                    }

                    Loges.LoggingProcess(level: LogLevel.INFO,
                        message: $"Deleted position {Id}, Type {Type}");

                    return true;
                }
                catch (Exception ex)
                {
                    await MessageBoxManager.GetMessageBoxStandard("Message", "Удалите записи связанные сним").ShowWindowAsync();

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
