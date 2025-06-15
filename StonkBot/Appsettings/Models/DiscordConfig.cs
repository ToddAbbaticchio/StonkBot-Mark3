namespace StonkBot.Appsettings.Models;

public class DiscordConfig
{
    public string VolumeAlertWebhook { get; set; } = null!;
    public string UpperShadowWebhook { get; set; } = null!;
    public string FourHandWebhook { get; set; } = null!;
    public string IpoWebhook { get; set; } = null!;
    public string EarningsReportWebhook { get; set; } = null!;
}