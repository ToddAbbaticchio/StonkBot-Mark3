// ReSharper disable InconsistentNaming
#pragma warning disable IDE1006

namespace StonkBot.Data.Entities;

public class AuthToken
{
    public int Id { get; set; }
    public string access_token { get; set; } = null!;
    public string refresh_token { get; set; } = null!;
    public string token_type { get; set; } = null!;
    public string scope { get; set; } = null!;
    public DateTime tokenCreatedTime { get; set; } = DateTime.UtcNow;
    public int expires_in { get; set; }
    public int refresh_token_expires_in { get; set; }
}