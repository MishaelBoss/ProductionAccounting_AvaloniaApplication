using System.Net;
using System.Net.NetworkInformation;

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
        catch
        {
            return false;
        }
    }

    public static bool ConnectToDataBase()
    {
        try
        {
            using (var ping = new Ping())
            {
                string hostName = "192.168.0.108";
                PingReply reply = ping.Send(hostName);

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

                    return true;
                }
                else
                {
                    return false;
                }

            }
        }
        catch
        {
            return false;
        }
    }

    public static void Reconnect()
    {
        Connect();
    }
}
