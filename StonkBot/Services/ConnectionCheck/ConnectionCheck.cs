using StonkBot.Services.ConsoleWriter;
using System.Net.NetworkInformation;

namespace StonkBot.Services.ConnectionCheck;

public interface IConnectionChecker
{
    Task<bool> DcTest();
}

public class ConnectionChecker : IConnectionChecker
{
    private readonly IConsoleWriter _con;

    public ConnectionChecker(IConsoleWriter con)
    {
        _con = con;
    }

    public async Task<bool> DcTest()
    {
        using var ping = new Ping();
        var reply = await ping.SendPingAsync("8.8.8.8");
        return reply.Status != IPStatus.Success;
    }
}