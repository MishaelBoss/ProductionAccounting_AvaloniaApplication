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
            
            NpgsqlTransaction? transaction = null;
            if (_additionalQueries is not null && _additionalQueries.Length > 0)
            {
                transaction = await connection.BeginTransactionAsync();
            }

            try
            {
                var totalRowsAffected = 0;

                if (_additionalQueries is not null && _additionalQueries.Length > 0)
                {
                    foreach (var query in _additionalQueries)
                    {
                        try
                        {
                            await using var command = new NpgsqlCommand(query, connection, transaction);
                            command.Parameters.AddWithValue("@id", Id);
                            var rows = await command.ExecuteNonQueryAsync();
                            totalRowsAffected += rows;
                            
                            Loges.LoggingProcess(level: LogLevel.Debug,
                                message: $"Additional query executed: {query}, rows affected: {rows}");
                        }
                        catch (Exception ex)
                        {
                            Loges.LoggingProcess(level: LogLevel.Error,
                                ex: ex,
                                message: $"Failed to execute additional query: {query}");
                            throw;
                        }
                    }
                }

                await using (var mainCommand = new NpgsqlCommand(_deleteSql, connection))
                {
                    if (transaction is not null)
                    {
                        mainCommand.Transaction = transaction;
                    }
                    
                    mainCommand.Parameters.AddWithValue("@id", Id);
                    var mainRows = await mainCommand.ExecuteNonQueryAsync();
                    totalRowsAffected += mainRows;
                    
                    Loges.LoggingProcess(level: LogLevel.Debug,
                        message: $"Main query executed: {_deleteSql}, rows affected: {mainRows}");
                }

                if (transaction is not null)
                {
                    await transaction.CommitAsync();
                }

                if (totalRowsAffected > 0)
                {
                    Loges.LoggingProcess(level: LogLevel.Info,
                        message: $"Successfully deleted records. Total rows affected: {totalRowsAffected}");

                    _onSuccessCallback.Invoke();
                    _window?.Close();
                }
                else
                {
                    Loges.LoggingProcess(level: LogLevel.Warning,
                        message: $"No records found to delete for ID: {Id}");
                    
                    _window?.Close();
                }
            }
            catch (Exception)
            {
                if (transaction is not null)
                {
                    await transaction.RollbackAsync();
                }
                throw;
            }
        }
        catch (PostgresException pgEx)
        {
            Loges.LoggingProcess(
                level: LogLevel.Error,
                ex: pgEx,
                message: $"Database error (SQLState: {pgEx.SqlState}): {pgEx.MessageText}");
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Error, ex: ex);
        }
    }
}
