using CommunityToolkit.Mvvm.Input;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels;

public class ConfirmDeleteWindowViewModel(double id, string title, string deleteSql, Action onSuccessCallback, string[]? additionalQueries = null) : ViewModelBase
{
    private ConfirmDeleteWindow? _window;
    private readonly Action _onSuccessCallback = onSuccessCallback;
    private readonly string _deleteSql = deleteSql;
    private readonly string[]? _additionalQueries = additionalQueries;

    private double Id { get; } = id;
    public string Title { get; set; } = title;

    public void SetWindow(ConfirmDeleteWindow window)
    {
        this._window = window;
    }

    public ICommand CancelCommand
        => new RelayCommand(() => _window?.Close());

    public ICommand DeleteCommand
        => new RelayCommand(async void () =>
        {
            try
            {
                await DeleteAsync();
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.Critical,
                    ex: ex,
                    message: "Failed to delete");
            }
        });

    private async Task DeleteAsync()
    {
        try
        {
            await using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                var rowsAffected = 0;
                if (_additionalQueries != null)
                {
                    try
                    {
                        foreach (var querty in _additionalQueries)
                        {
                            await using var command = new NpgsqlCommand(querty, connection, transaction);
                            command.Parameters.AddWithValue("@id", Id);
                            await command.ExecuteNonQueryAsync();
                        }

                        await transaction.CommitAsync();
                    }
                    catch (PostgresException ex)
                    {
                        await transaction.RollbackAsync();

                        Loges.LoggingProcess(level: LogLevel.Warning,
                            ex: ex);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();

                        Loges.LoggingProcess(level: LogLevel.Warning,
                            ex: ex);
                        throw;
                    }
                }

                await using (var command2 = new NpgsqlCommand(_deleteSql, connection))
                {
                    command2.Parameters.AddWithValue("@id", Id);
                    rowsAffected += await command2.ExecuteNonQueryAsync();
                }

                if (rowsAffected > 0)
                {
                    Loges.LoggingProcess(level: LogLevel.Info,
                        message: $"Deleted record with ID: {Id}");

                    _onSuccessCallback.Invoke();
                    _window?.Close();
                }
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.Error,
                    ex: ex);
            }
        }
        catch (PostgresException ex) 
        {
            Loges.LoggingProcess(
                level: LogLevel.Warning, 
                ex: ex,
                message: $"Error DB (SQLState: {ex.SqlState}): {ex.MessageText}" );
                            
            Loges.LoggingProcess(
                level: LogLevel.Warning, 
                ex: ex,
                message: $"Error DB (Detail: {ex.Detail})" );
            
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
