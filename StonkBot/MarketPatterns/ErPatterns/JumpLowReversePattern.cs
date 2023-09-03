using StonkBot.Data.Entities;
using StonkBot.Data.Enums;
using StonkBot.Extensions;
using StonkBot.MarketPatterns.Models;

namespace StonkBot.MarketPatterns;

public partial interface IMarketPatternMatcher
{
    Task<List<AlertData>> JumpLowReverseCheck(EarningsReport er, HistoricalData erDay, HistoricalData erDayAfter, List<HistoricalData> checkRange, CancellationToken cToken);
}

public partial class MarketPatternMatcher
{
    public async Task<List<AlertData>> JumpLowReverseCheck(EarningsReport er, HistoricalData erDay, HistoricalData erDayAfter, List<HistoricalData> checkRange, CancellationToken cToken)
    {
        var newAlerts = new List<AlertData>();
        var prevJumpLowReverseAlertDates = er.Alerts
            .Where(x => x.Symbol == er.Symbol)
            .Where(x => x.Type == $"{AlertType.JumpLowReverseAlert}")
            .Select(x => x.Date)
            .ToList();
        var jumpLowReverseDates = checkRange
            .Where(x => x.Date > erDayAfter.Date)
            .Where(x => x.Close >= erDay.High)
            .ToList();
        var newJumpLowReverseAlerts = jumpLowReverseDates
            .Where(x => !prevJumpLowReverseAlertDates.Contains(x.Date))
            .ToList();
        foreach (var alertDay in newJumpLowReverseAlerts)
        {
            newAlerts.Add(new AlertData
            {
                AlertType = AlertType.JumpLowReverseAlert,
                Symbol = erDay.Symbol,
                Sector = erDay.IndustryInfo?.Sector,
                Industry = erDay.IndustryInfo?.Industry,
                Category = erDay.IndustryInfo?.Category,
                IsWatched = await er.IsWatched(),
                Date = alertDay.Date,
                Message = ",Jump low - reverse"
            });
        }

        return newAlerts;
    }
}