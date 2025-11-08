using Npgsql;
using ProductionAccounting_AvaloniaApplication.Models;
using ProductionAccounting_AvaloniaApplication.Scripts;
using System;
using System.IO;
using System.Text.Json;

namespace ProductionAccounting_AvaloniaApplication.Script;

public class ManagerCookie
{
    public static Double GETIDUSER { get; set; }
    public static string? GETUSERNAME { get; set; } = string.Empty;
    public static string? GETFIRSTNAME { get; set; } = string.Empty;
    public static string? GETLASTNAME { get; set; } = string.Empty;

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

            try
            {
                string query = "SELECT id, username, first_name, last_name FROM public.\"user\" WHERE id = @id";

                using (var connection = new NpgsqlConnection(Arguments.connection))
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", data.Id);

                        using (var reader = command.ExecuteReader())
                        {
                            try
                            {
                                if (!reader.Read()) return false;

                                double dbid = reader.GetDouble(0);
                                string dbusername = reader.GetString(1);
                                string dbfist_name = reader.GetString(2);
                                string dblast_name = reader.GetString(3);

                                GETIDUSER = dbid;
                                GETUSERNAME = dbusername;
                                GETFIRSTNAME = dbfist_name;
                                GETLASTNAME = dblast_name;
                                return dbusername == data.Username;
                            }
                            catch
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.ERROR,
                    ex: ex,
                    message: "error to connect db");
            }

            return data?.Expires > DateTime.Now;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsSuperUser()
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

            try
            {
                string query = "SELECT is_superuser FROM public.\"user\" WHERE id = @id";

                using (var connection = new NpgsqlConnection(Arguments.connection))
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", data.Id);

                        using (var reader = command.ExecuteReader())
                        {
                            try
                            {
                                if (!reader.Read()) return false;

                                bool dbis_superuser = reader.GetBoolean(0);

                                return dbis_superuser;
                            }
                            catch
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.ERROR,
                    ex: ex,
                    message: "error to connect db");
            }

            return data?.Expires > DateTime.Now;
        }
        catch
        {
            return false;
        }
    }

    public static bool IsMetrologist()
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

            try
            {
                string query = "SELECT is_metrologist FROM public.\"user\" WHERE id = @id";

                using (var connection = new NpgsqlConnection(Arguments.connection))
                {
                    connection.Open();
                    using (var command = new NpgsqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@id", data.Id);

                        using (var reader = command.ExecuteReader())
                        {
                            try
                            {
                                if (!reader.Read()) return false;

                                bool dbis_metrologist = reader.GetBoolean(0);

                                return dbis_metrologist;
                            }
                            catch
                            {
                                return false;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Loges.LoggingProcess(level: LogLevel.ERROR,
                    ex: ex,
                    message: "error to connect db");
            }

            return data?.Expires > DateTime.Now;
        }
        catch
        {
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
