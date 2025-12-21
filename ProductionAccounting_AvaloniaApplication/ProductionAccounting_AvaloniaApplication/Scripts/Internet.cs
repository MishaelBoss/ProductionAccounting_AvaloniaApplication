using System;
using System.Net.NetworkInformation;
using Npgsql;

namespace ProductionAccounting_AvaloniaApplication.Scripts;

internal abstract class Internet
{
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
                Loges.LoggingProcess(LogLevel.Warning, 
                    "Database connection data is incomplete");
                return false;
            }

            using var ping = new Ping();
            var hostName = Arguments.Ip;
            var reply = ping.Send(hostName, 3000);

            Loges.LoggingProcess(level: LogLevel.Info,
                message: $"Ping status for ({hostName}): {reply.Status}");

            if (reply is { Status: IPStatus.Success })
            {
                Loges.LoggingProcess(level: LogLevel.Info,
                    message: $"Address: {reply.Address}");

                Loges.LoggingProcess(level: LogLevel.Info,
                    message: $"Roundtrip time: {reply.RoundtripTime}");

                Loges.LoggingProcess(level: LogLevel.Info,
                    message: $"Time to live: {reply.Options?.Ttl}");

                return TestPostgreSqlConnection();
            }
            else
            {
                return false;
            }
        }
        catch (PingException pex)
        {
            Loges.LoggingProcess(LogLevel.Warning, 
                $"Ping failed: {pex.Message}");
            return false;
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.Error, 
                $"Connection test failed: {ex.Message}");
            return false;
        }
    }

    private static bool TestPostgreSqlConnection()
    {
        try
        {
            using var connection = new NpgsqlConnection($"Server={Arguments.Ip};Port={Arguments.Port};Database={Arguments.Database};User Id={Arguments.User};Password={Arguments.Password};Timeout=5");
            connection.Open();

            using var cmd = new NpgsqlCommand("SELECT 1", connection);
            var result = cmd.ExecuteScalar();
            Loges.LoggingProcess(LogLevel.Info, 
                $"PostgreSQL connection test successful, result {result}");
            return true;
        }
        catch (Exception ex)
        {
            Loges.LoggingProcess(LogLevel.Warning, 
                $"PostgreSQL connection failed: {ex.Message}");
            return false;
        }
    }
}
