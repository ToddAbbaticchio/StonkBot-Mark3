using StonkBot.Data.Enums;
using StonkBot.Extensions;

namespace StonkBot.MarketPatterns.Models;

public class AlertData
{
    public AlertType AlertType { get; set; }
    public string Symbol { get; set; } = null!;
    public string? Sector { get; set; }
    public string? Industry { get; set; }
    public string? Category { get; set; }
    public string? IsWatched { get; set; }
    public DateTime Date { get; set; }
    public string Type { get; set; } = null!;
    public string Message { get; set; } = null!;
}