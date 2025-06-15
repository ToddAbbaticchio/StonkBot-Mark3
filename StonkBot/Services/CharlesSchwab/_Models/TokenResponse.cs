namespace StonkBot.Services.CharlesSchwab._Models;

public class TokenResponse
{
    #pragma warning disable IDE1006
    public DateTime tokenCreatedTime { get; set; } = DateTime.UtcNow;
    public string access_token { get; set; } = null!;
    public string? refresh_token { get; set; }
    public int expires_in { get; set; }
    public string? scope { get; set; }
    #pragma warning restore IDE1006
}