using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;

namespace ProductionAccounting_AvaloniaApplication.ViewModels;

public class ConfirmDeleteUserWindowViewModel : ViewModelBase
{
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

    public bool Delete()
    {
        try
        {
            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                connection.Open();
                try
                {
                    int rowsAffected = 0;

                    string sql1 = "DELETE FROM public.user_to_user_type WHERE user_id = @id";
                    using (var command1 = new NpgsqlCommand(sql1, connection))
                    {
                        command1.Parameters.AddWithValue("@id", UserID);
                        rowsAffected += command1.ExecuteNonQuery();
                    }

                    string sql2 = "DELETE FROM public.user_to_departments WHERE user_id = @id";
                    using (var command1 = new NpgsqlCommand(sql2, connection))
                    {
                        command1.Parameters.AddWithValue("@id", UserID);
                        rowsAffected += command1.ExecuteNonQuery();
                    }

                    string sql3 = "DELETE FROM public.user_to_position WHERE user_id = @id";
                    using (var command1 = new NpgsqlCommand(sql3, connection))
                    {
                        command1.Parameters.AddWithValue("@id", UserID);
                        rowsAffected += command1.ExecuteNonQuery();
                    }

                    string sql4 = "DELETE FROM public.user WHERE id = @id";
                    using (var command2 = new NpgsqlCommand(sql4, connection))
                    {
                        command2.Parameters.AddWithValue("@id", UserID);
                        rowsAffected += command2.ExecuteNonQuery();
                    }

                    Loges.LoggingProcess(level: LogLevel.INFO,
                        message: $"Deleted user {UserID}, Login {Login}");

                    return rowsAffected > 1;
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
