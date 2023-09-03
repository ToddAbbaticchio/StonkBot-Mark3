// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Global
#pragma warning disable IDE1006
#pragma warning disable CS8618
namespace StonkBot.Services.TDAmeritrade.StreamingClient.Models;


public class HeartbeatSignal
{
    public Notify[] notify { get; set; }
}

public class Notify
{
    public string heartbeat { get; set; }
}
