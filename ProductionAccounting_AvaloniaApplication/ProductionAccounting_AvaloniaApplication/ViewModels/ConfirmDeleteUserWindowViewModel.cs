using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels;

public class ConfirmDeleteUserWindowViewModel : ViewModelBase
{
    private ConfirmDeleteUserWindow? window { get; set; } = null;

    public void SetWindow(ConfirmDeleteUserWindow _window)
    {
        window = _window;
    }

    public ICommand CancelCommand
        => new RelayCommand(() => window?.Close());

    public ICommand DeleteCommand
        => new RelayCommand(async () => await DeleteAsync());

    private double _userId = 0;
    public double UserID
    {
        get => _userId;
        set => this.RaiseAndSetIfChanged(ref _userId, value);
    }

    private string _login = string.Empty;
    public string Login
    {
        get => _login;
        set => this.RaiseAndSetIfChanged(ref _login, value);
    }

    private async Task DeleteAsync()
    {
        try
        {
            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                try
                {
                    int rowsAffected = 0;

                    var deleteQueries = new[]
                    {
                        "DELETE FROM public.timesheet WHERE user_id = @id",
                        "DELETE FROM public.user_to_user_type WHERE user_id = @id",
                        "DELETE FROM public.user_to_departments WHERE user_id = @id",
                        "DELETE FROM public.user_to_position WHERE user_id = @id",
                        "DELETE FROM public.production WHERE user_id = @id"
                    };

                    foreach (var querty in deleteQueries) 
                    {
                        await using (var command = new NpgsqlCommand(querty, connection)) 
                        {
                            command.Parameters.AddWithValue("@id", UserID);
                            await command.ExecuteNonQueryAsync();
                        }
                    }

                    using (var command2 = new NpgsqlCommand("DELETE FROM public.user WHERE id = @id", connection))
                    {
                        command2.Parameters.AddWithValue("@id", UserID);
                        rowsAffected += await command2.ExecuteNonQueryAsync();
                    }

                    if (rowsAffected > 0) 
                    {
                        Loges.LoggingProcess(level: LogLevel.INFO,
                            message: $"Deleted user {UserID}, Login {Login}");

                        WeakReferenceMessenger.Default.Send(new RefreshUserListMessage());
                        window?.Close();
                    }
                }
                catch (Exception ex)
                {
                    Loges.LoggingProcess(level: LogLevel.ERROR,
                        ex: ex);
                }
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.ERROR,
                ex: ex);
        }
    }
}
