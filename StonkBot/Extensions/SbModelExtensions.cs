using System.CodeDom;
using StonkBot.Data.Entities;
using StonkBot.MarketPatterns.Models;
using StonkBot.Services.DiscordService.Models;

namespace StonkBot.Extensions;

public static class SbModelExtensions
{
    public static string GetMessageString(this AlertData x)
    {
        return $"{x.Sector},{x.Industry},{x.Category},{x.Symbol},{x.IsWatched},{x.Date.SbDateString()},{x.Message}";
    }

    public static List<string> ToTableMessage(this AlertData x, string type)
    {
        switch (type)
        {
            case "All":
                return new List<string> { x.Sector ?? string.Empty, x.Industry ?? string.Empty, x.Category ?? string.Empty, x.Symbol, x.IsWatched ?? string.Empty, x.Date.SbDateString(), x.Message };

            case "Watched":
                return new List<string> { x.Symbol, x.Sector ?? string.Empty, x.Message };

            default:
                return new();
        }
    }

    public static ErAlert GenerateErAlert(this AlertData x)
    {
        return new ErAlert
        {
            Symbol = x.Symbol,
            Date = x.Date,
            Type = $"{x.AlertType}"
        };
    }

    public static bool ContainsData(this DiscordTableMessage x)
    {
        return x.Data.Count > 1;
    }
}