using StonkBot.Data.Entities;
using StonkBot.Data.Enums;
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
        if (er.Alerts.Any(x => x.Type == $"{AlertType.JumpLowReverseAlert}"))
            return newAlerts;

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
            .OrderBy(x => x.Date)
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
                IsWatched = await _db.IsWatched(er.Symbol, cToken),
                Date = alertDay.Date,
                Message = "Jump low - reverse"
            });

            break;
        }

        return newAlerts;
    }
}