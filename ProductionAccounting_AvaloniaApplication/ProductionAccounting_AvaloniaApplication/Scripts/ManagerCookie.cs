using MsBox.Avalonia;
using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using System;
using System.IO;
using System.Text.Json;

namespace ProductionAccounting_AvaloniaApplication.Scripts;

public class ManagerCookie
{
    public static Double? GetIdUser { get; set; } = 0;
    public static string? GetLogin { get; set; } = string.Empty;
    public static string? GetFirstName { get; set; } = string.Empty;
    public static string? GetLastName { get; set; } = string.Empty;
    public static string? GetMiddleName { get; set; } = string.Empty;
    public static bool IsAdministrator { get; private set; }
    public static bool IsManager { get; private set; }
    public static bool IsMaster { get; private set; }
    public static bool IsEmployee { get; private set; }
    public static double? CurrentUserTypeId { get; private set; }

    public static void SaveLoginCookie(double id, string username, string token, DateTime expires, string path)
    {
        var data = new
        {
            Id = id,
            Username = username,
            Token = token,
            Expires = expires
        };

        string cookieFilePath = Path.Combine(path, "login.cookie");
        File.WriteAllText(cookieFilePath, JsonSerializer.Serialize(data));
    }

    public static bool IsUserLoggedIn()
    {
        try
        {
            if (!Directory.Exists(Paths.SharedFolder)) Directory.CreateDirectory(Paths.SharedFolder);

            string cookieFilePath = Path.Combine(Paths.SharedFolder, "login.cookie");

            if (!File.Exists(cookieFilePath))
                return false;

            var json = File.ReadAllText(cookieFilePath);
            var data = JsonSerializer.Deserialize<CookieServer>(json);

            if (data == null || data.Expires <= DateTime.Now)
                return false;

            using (var connection = new NpgsqlConnection(Arguments.connection))
            {
                connection.Open();

                double? id = 0;
                string? login = string.Empty;
                string? first_name = string.Empty;
                string? last_name = string.Empty;
                string? middle_name = string.Empty;
                double? user_type_id = 0;
                string? user_type_name = string.Empty;

                try
                {
                    string sql1 = "SELECT id, login, first_name, last_name, middle_name FROM public.\"user\" WHERE id = @id";
                    using (var command = new NpgsqlCommand(sql1, connection))
                    {
                        command.Parameters.AddWithValue("@id", data.Id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                id = reader.IsDBNull(0) ? 0 : reader.GetDouble(0);
                                login = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                                first_name = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                                last_name = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);
                                middle_name = reader.IsDBNull(4) ? string.Empty : reader.GetString(4);

                                GetIdUser = id;
                                GetLogin = login;
                                GetFirstName = first_name;
                                GetLastName = last_name;
                                GetMiddleName = middle_name;
                            }
                            else 
                            {
                                Loges.LoggingProcess(level: LogLevel.ERROR,
                                        "No found user");
                                return false;
                            }
                        }
                    }

                    string sql2 = "SELECT user_type_id FROM public.\"user_to_user_type\" WHERE user_id = @id";
                    using (var command = new NpgsqlCommand(sql2, connection))
                    {
                        command.Parameters.AddWithValue("@id", data.Id);

                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                user_type_id = reader.IsDBNull(0) ? 0 : reader.GetDouble(0);

                                CurrentUserTypeId = user_type_id;

                                IsAdministrator = user_type_id == 1;
                                IsManager = user_type_id == 2;
                                IsMaster = user_type_id == 3;
                                IsEmployee = user_type_id == 4;

                            }
                            else
                            {
                                user_type_id = 0;
                                CurrentUserTypeId = null;
                                IsAdministrator = false;
                                IsManager = false;
                                IsMaster = false;
                                IsEmployee= false;
                            }
                        }
                    }

/*                    if (user_type_id > 0)
                    {
                        string sql3 = "SELECT type_user FROM public.\"user_type\" WHERE id = @id";
                        using (var command = new NpgsqlCommand(sql3, connection))
                        {
                            command.Parameters.AddWithValue("@id", user_type_id);

                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    user_type_name = reader.IsDBNull(0) ? string.Empty : reader.GetString(0);
                                }
                            }
                        }
                    }*/

                    return login == data.Username;
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

    public static void DeleteCookie()
    {
        string test = Path.Combine(Paths.SharedFolder, "login.cookie");

        if (File.Exists(test))
            File.Delete(test);
    }
}
