using CommunityToolkit.Mvvm.Input;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ProductionAccounting_AvaloniaApplication.ViewModels;

public class ConfirmDeleteWindowViewModel(double id, string title, string deleteSql, Action onSuccessCallback, string[]? additionalQueries = null) : ViewModelBase
{
    private ConfirmDeleteWindow? window;
    private readonly Action onSuccessCallback = onSuccessCallback;
    private readonly string deleteSql = deleteSql;
    private readonly string[]? additionalQueries = additionalQueries;

    public double Id { get; set; } = id;
    public string Title { get; set; } = title;

    public void SetWindow(ConfirmDeleteWindow _window)
    {
        window = _window;
    }

    public ICommand CancelCommand
        => new RelayCommand(() => window?.Close());

    public ICommand DeleteCommand
        => new RelayCommand(async () => await DeleteAsync());

    public async Task DeleteAsync()
    {
        try
        {
            using var connection = new NpgsqlConnection(Arguments.Connection);
            await connection.OpenAsync();
            await using var transaction = await connection.BeginTransactionAsync();
            try
            {
                int rowsAffected = 0;
                if (additionalQueries != null)
                {
                    try
                    {
                        foreach (var querty in additionalQueries)
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

                        Loges.LoggingProcess(level: LogLevel.WARNING,
                            ex: ex);
                        throw;
                    }
                    catch (Exception ex)
                    {
                        await transaction.RollbackAsync();

                        Loges.LoggingProcess(level: LogLevel.WARNING,
                            ex: ex);
                        throw;
                    }
                }

                using (var command2 = new NpgsqlCommand(deleteSql, connection))
                {
                    command2.Parameters.AddWithValue("@id", Id);
                    rowsAffected += await command2.ExecuteNonQueryAsync();
                }

                if (rowsAffected > 0)
                {
                    Loges.LoggingProcess(level: LogLevel.INFO,
                        message: $"Deleted record with ID: {Id}");

                    onSuccessCallback?.Invoke();
                    window?.Close();
                }
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.ERROR,
                    ex: ex);
            }
        }
        catch (PostgresException ex) 
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING,
                ex: ex);
        }
    }
}
