using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;

namespace ProductionAccounting_AvaloniaApplication.ViewModels;

public class ConfirmDeleteProductWindowViewModel : ViewModelBase
{
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

    public bool Delete()
    {
        try
        {
            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                connection.Open();
                try
                {
                    string sql1 = "DELETE FROM public.product WHERE id = @id";
                    using (var command1 = new NpgsqlCommand(sql1, connection))
                    {
                        command1.Parameters.AddWithValue("@id", Id);
                        command1.ExecuteNonQuery();
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
