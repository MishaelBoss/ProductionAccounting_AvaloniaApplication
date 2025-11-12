using Npgsql;
using ProductionAccounting_AvaloniaApplication.Scripts;
using ReactiveUI;
using System;
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

    private string _position = string.Empty;
    public string Position
    {
        get => _position;
        set => this.RaiseAndSetIfChanged(ref _position, value);
    }

    private string _employee = string.Empty;
    public string Employee
    {
        get => _employee;
        set => this.RaiseAndSetIfChanged(ref _employee, value);
    }

    private string _department = string.Empty;
    public string Department
    {
        get => _department;
        set => this.RaiseAndSetIfChanged(ref _department, value);
    }

    private string _email = string.Empty;
    public string Email
    {
        get => _email;
        set => this.RaiseAndSetIfChanged(ref _email, value);
    }

    private string _phone = string.Empty;
    public string Phone
    {
        get => _phone;
        set => this.RaiseAndSetIfChanged(ref _phone, value);
    }

    public async Task LoadListTypeToComboBoxAsync()
    {
        if (!ManagerCookie.IsUserLoggedIn() && ManagerCookie.GetIdUser != 0) return;

        try
        {
            string sql = @"
            SELECT 
                ut.type_user,
                d.type as department,
                p.type as position
            FROM public.user_to_user_type utt
            LEFT JOIN public.user_type ut ON utt.user_type_id = ut.id
            LEFT JOIN public.user_to_departments ud ON ud.user_id = utt.user_id
            LEFT JOIN public.departments d ON ud.department_id = d.id
            LEFT JOIN public.user_to_position up ON up.user_id = utt.user_id
            LEFT JOIN public.positions p ON up.position_id = p.id
            WHERE utt.user_id = @userID";

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                await connection.OpenAsync();

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@userID", ManagerCookie.GetIdUser ?? 0);

                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            //UserType = reader.IsDBNull(0) ? "none" : reader.GetString(0);
                            Department = reader.IsDBNull(1) ? "none" : reader.GetString(1);
                            Position = reader.IsDBNull(2) ? "none" : reader.GetString(2);
                        }
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.WARNING, 
                ex: ex);
        }
    }

    public async Task LoadDateAsync() 
    {
        if (!ManagerCookie.IsUserLoggedIn() && ManagerCookie.GetIdUser != 0) return;

        try
        {
            string sql = @"SELECT login, first_name, last_name, middle_name, email, phone, employee_id FROM public.""user"" WHERE id = @id";

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
                            Login = reader.IsDBNull(0) ? "none" : reader.GetString(0);
                            FirstName = reader.IsDBNull(1) ? "none" : reader.GetString(1);
                            LastName = reader.IsDBNull(2) ? "none" : reader.GetString(2);
                            MiddleName = reader.IsDBNull(3) ? "none" : reader.GetString(3);
                            Email = reader.IsDBNull(4) ? "none" : reader.GetString(4);
                            Phone = reader.IsDBNull(5) ? "none" : reader.GetString(5);
                            Employee = reader.IsDBNull(6) ? "none" : reader.GetString(6);
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
