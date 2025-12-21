using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using System;
using System.IO;
using System.Text.Json;

namespace ProductionAccounting_AvaloniaApplication.Scripts;

public static class ManagerCookie
{
    public static double? GetIdUser { get; private set; } = 0;
    public static string? GetFirstName { get; private set; } = string.Empty;
    public static string? GetLastName { get; private set; } = string.Empty;
    public static bool IsAdministrator { get; private set; }
    public static bool IsManager { get; private set; }
    public static bool IsMaster { get; private set; }
    public static bool IsEmployee { get; private set; }

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

            if (!Internet.ConnectToDataBase()) return false;

            using var connection = new NpgsqlConnection(Arguments.Connection);
            connection.Open();

            try
            {
                string sql1 = "SELECT id, login, first_name, last_name, middle_name FROM public.\"user\" WHERE id = @id AND is_active = true";
                string login;
                using (var command = new NpgsqlCommand(sql1, connection))
                {
                    command.Parameters.AddWithValue("@id", data.Id);

                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        double? id = reader.IsDBNull(0) ? 0 : reader.GetDouble(0);
                        login = reader.IsDBNull(1) ? string.Empty : reader.GetString(1);
                        var firstName = reader.IsDBNull(2) ? string.Empty : reader.GetString(2);
                        var lastName = reader.IsDBNull(3) ? string.Empty : reader.GetString(3);

                        GetIdUser = id;
                        GetFirstName = firstName;
                        GetLastName = lastName;
                    }
                    else
                    {
                        Loges.LoggingProcess(level: LogLevel.Error,
                                "No found user");
                        return false;
                    }
                }

                const string sql2 = "SELECT user_type_id FROM public.user_to_user_type WHERE user_id = @id";
                using (var command = new NpgsqlCommand(sql2, connection))
                {
                    command.Parameters.AddWithValue("@id", data.Id);

                    using var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        int userTypeId = reader.IsDBNull(0) ? 0 : reader.GetInt32(0);

                        IsAdministrator = userTypeId == 1;
                        IsManager = userTypeId == 2;
                        IsMaster = userTypeId == 3;
                        IsEmployee = userTypeId == 4;

                    }
                    else
                    {
                        IsAdministrator = IsManager = IsMaster = IsEmployee = false;
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
                Loges.LoggingProcess(level: LogLevel.Error,
                    ex: ex);
                return false;
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(level: LogLevel.Error,
                ex: ex);
            return false;
        }
    }

    public static void DeleteCookie()
    {
        var pathToCookie = Path.Combine(Paths.SharedFolder, "login.cookie");

        if (File.Exists(pathToCookie))
            File.Delete(pathToCookie);
    }
}
