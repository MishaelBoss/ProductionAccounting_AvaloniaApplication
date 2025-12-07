using System;
using System.Net;
using System.Net.NetworkInformation;
using Npgsql;

namespace ProductionAccounting_AvaloniaApplication.Scripts;

internal abstract class Internet
{
    public static bool Connect()
    {
        try
        {
            Dns.GetHostEntry("dotnet.beget.tech");
            return true;
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.ERROR, 
                $"Connection test failed: {ex.Message}");
            return false;
        }
    }

    public static bool ConnectToDataBase()
    {
        try
        {
            if  (string.IsNullOrEmpty(Arguments.Ip) || 
                string.IsNullOrEmpty(Arguments.Port) || 
                string.IsNullOrEmpty(Arguments.Database) || 
                string.IsNullOrEmpty(Arguments.User) || 
                string.IsNullOrEmpty(Arguments.Password))
            {
                Loges.LoggingProcess(LogLevel.WARNING, 
                    "Database connection data is incomplete");
                return false;
            }

            using (var ping = new Ping())
            {
                string hostName = Arguments.Ip;
                PingReply reply = ping.Send(hostName, 3000);

                Loges.LoggingProcess(level: LogLevel.INFO,
                        message: $"Ping status for ({hostName}): {reply.Status}");

                if (reply is { Status: IPStatus.Success })
                {
                    Loges.LoggingProcess(level: LogLevel.INFO,
                        message: $"Address: {reply.Address}");

                    Loges.LoggingProcess(level: LogLevel.INFO,
                        message: $"Roundtrip time: {reply.RoundtripTime}");

                    Loges.LoggingProcess(level: LogLevel.INFO,
                        message: $"Time to live: {reply.Options?.Ttl}");

                    return TestPostgreSqlConnection();
                }
                else
                {
                    return false;
                }

            }
        }
        catch (PingException pex)
        {
            Loges.LoggingProcess(LogLevel.WARNING, 
                $"Ping failed: {pex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.ERROR, 
                $"Connection test failed: {ex.Message}");
            return false;
        }
    }

    private static bool TestPostgreSqlConnection()
    {
        try
        {
            using (var connection = new NpgsqlConnection($"Server={Arguments.Ip};Port={Arguments.Port};Database={Arguments.Database};User Id={Arguments.User};Password={Arguments.Password};Timeout=5"))
            {
                connection.Open();
                
                using (var cmd = new NpgsqlCommand("SELECT 1", connection))
                {
                    var result = cmd.ExecuteScalar();
                    Loges.LoggingProcess(LogLevel.INFO, 
                        "PostgreSQL connection test successful");
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.WARNING, 
                $"PostgreSQL connection failed: {ex.Message}");
            return false;
        }
    }
}
