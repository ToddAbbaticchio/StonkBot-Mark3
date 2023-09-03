using StonkBot.Data.Entities;
using StonkBot.MarketPatterns.Models;

namespace StonkBot.Extensions;

public static class SbModelExtensions
{
    public static string GetMessageString(this AlertData x)
    {
        return $"{x.Sector},{x.Industry},{x.Category},{x.Symbol},{x.IsWatched},{x.Date.SbDateString()},{x.Message}";
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
}