namespace StonkBot.Services.TDAmeritrade._SharedModels;
public class StreamCredential
{
    public string AccountId { get; set; } = null!;
    public string Token { get; set; } = null!;
    public string Company { get; set; } = null!;
    public string Segment { get; set; } = null!;
    public string CdDomain { get; set; } = null!;
    public string UserGroup { get; set; } = null!;
    public string AccessLevel { get; set; } = null!;
    public string Authorized { get; set; } = null!;
    public double Timestamp { get; set; }
    public string AppId { get; set; } = null!;
    public string Acl { get; set; } = null!;
}