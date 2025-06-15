#pragma warning disable IDE1006

namespace StonkBot.Data.Entities;

public class AuthToken
{
    public int Id { get; set; }
    public string access_token { get; set; } = null!;
    public string refresh_token { get; set; } = null!;
    public string token_type { get; set; } = null!;
    public string scope { get; set; } = null!;
    public DateTime tokenCreatedTime { get; set; } = DateTime.Now.ToLocalTime();
    public int expires_in { get; set; }
    public int refresh_token_expires_in { get; set; }

    public bool IsStale() {
        if (expires_in == 0)
            return true;
        var expiryTime = tokenCreatedTime + TimeSpan.FromSeconds(expires_in - 300);
        var now = DateTime.Now.ToLocalTime();
        if (expiryTime <= now)
            return true;
        return false;
    }
}
