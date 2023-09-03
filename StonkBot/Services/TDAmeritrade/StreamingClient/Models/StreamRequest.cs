namespace StonkBot.Services.TDAmeritrade.StreamingClient.Models;
public class StreamRequest
{
    public string? service { get; set; }
    public string? command { get; set; }
    public string? requestid { get; set; }
    public string? account { get; set; }
    public string? source { get; set; }
    public Parameters? parameters { get; set; }
}

public class Parameters
{
    public string? credential { get; set; }
    public string? token { get; set; }
    public string? version { get; set; }
    public string? keys { get; set; }
    public string? fields { get; set; }
    public string? qoslevel { get; set; }
}
