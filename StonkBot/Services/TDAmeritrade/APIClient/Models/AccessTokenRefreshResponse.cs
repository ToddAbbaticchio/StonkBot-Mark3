using System.Diagnostics.CodeAnalysis;

namespace StonkBot.Services.TDAmeritrade.APIClient.Models;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class AccessTokenRefreshResponse
{
#pragma warning disable IDE1006 // Naming Styles
    public DateTime tokenCreatedTime { get; set; } = DateTime.UtcNow;
    public string access_token { get; set; } = null!;
    public string scope { get; set; } = null!;
    public int expires_in { get; set; }
    public string token_type { get; set; } = null!;
#pragma warning restore IDE1006 // Naming Styles
}