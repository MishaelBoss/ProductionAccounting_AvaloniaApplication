using MsBox.Avalonia;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System.Threading.Tasks;

namespace ProductionAccounting_AvaloniaApplication.ViewModels.Control;

public class ProfileUserUserControlViewModel : ViewModelBase
{
    private string _login = string.Empty;
    public string Login
    {
        get => _login;
        set => this.RaiseAndSetIfChanged(ref _login, value);
    }

    private string _firstName = string.Empty;
    public string FirstName 
    {
        get => _firstName;
        set => this.RaiseAndSetIfChanged(ref _firstName, value);
    }

    private string _lastName = string.Empty;
    public string LastName
    {
        get => _lastName;
        set => this.RaiseAndSetIfChanged(ref _lastName, value);
    }

    private string _middleName = string.Empty;
    public string MiddleName
    {
        get => _middleName;
        set => this.RaiseAndSetIfChanged(ref _middleName, value);
    }

    public async Task LoadDateAsync() 
    {
        if (!ManagerCookie.IsUserLoggedIn()) return;

        try
        {
            string sql = @"SELECT login, first_name, last_name, middle_name FROM public.""user"" WHERE id = @id";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", ManagerCookie.GetIdUser ?? 0);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            Login = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                            FirstName = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                            LastName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                            MiddleName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                        }
                    }
                }
            }
        }
        catch (NpgsqlException ex)
        {
            Loges.LoggingProcess(LogLevel.CRITICAL,
                "Connection or request error",
                ex: ex);
        }
    }
}
