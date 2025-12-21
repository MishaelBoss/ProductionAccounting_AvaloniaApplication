using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class AddPositionUserControlViewModel : ViewModelBase, INotifyPropertyChanged
{       
    private string _messageerror = string.Empty;
    public string Messageerror
    {
        get => _messageerror;
        set => this.RaiseAndSetIfChanged(ref _messageerror, value);
    }

    private string _position = string.Empty;
    public string Position
    {
        get => _position;
        set
        {
            this.RaiseAndSetIfChanged(ref _position, value);
            this.RaisePropertyChanged(nameof(IsActiveConfirmButton));
        }
    }

    public static ICommand CancelCommand
        => new RelayCommand(() => WeakReferenceMessenger.Default.Send(new OpenOrCloseAddPositionStatusMessage(false)));

    public ICommand ConfirmCommand
        => new RelayCommand(async () => await SaveAsync());

    public bool IsActiveConfirmButton
        => !string.IsNullOrEmpty(Position);

    public async Task SaveAsync()
    {
        try
        {
            if (string.IsNullOrEmpty(Position))
            {
                Messageerror = "Не все поля заполнены";
                return;
            }

            const string sql = "INSERT INTO public.positions (type) VALUES (@type)";

            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();
            await using var command = new NpgsqlCommand(sql, connection);

            command.Parameters.AddWithValue("@type", Position);
            await command.ExecuteNonQueryAsync();

            WeakReferenceMessenger.Default.Send(new RefreshPositionListMessage());
            WeakReferenceMessenger.Default.Send(new OpenOrCloseAddPositionStatusMessage(false));
        }
        catch (PostgresException ex)
        {
            Loges.LoggingProcess(
                level: LogLevel.Warning,
                ex: ex,
                message: $"Error DB (SQLState: {ex.SqlState}): {ex.MessageText}");

            Loges.LoggingProcess(
                level: LogLevel.Warning,
                ex: ex,
                message: $"Error DB (Detail: {ex.Detail})");

            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Warning,
                ex: ex);
        }
    }
}
