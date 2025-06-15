using AngleSharp.Text;

namespace StonkBot.Appsettings.Models;

public class SchwabApiConfig
{
    public string CallbackUrl { get; set; } = null!;
    public string AppKey { get; set; } = null!;
    public string AppSecret { get; set; } = null!;
    public string BaseUrl { get; set; } = null!;
    public string TokenUrl { get; set; } = null!;
    public string ClientId => AppKey;
}