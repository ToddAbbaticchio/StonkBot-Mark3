#pragma warning disable IDE1006
#pragma warning disable CS8618
namespace StonkBot.Services.CharlesSchwab.StreamingClient.Models;


public class StreamHeartbeat
{
    public Notify[] notify { get; set; }
}

public class Notify
{
    public string heartbeat { get; set; }
}
