using StonkBot.Data.Entities;
using StonkBot.Data.Enums;
using StonkBot.Extensions;
using StonkBot.MarketPatterns.Models;

namespace StonkBot.MarketPatterns;

public partial interface IMarketPatternMatcher
{
    Task<List<AlertData>> JumpLowCheck(EarningsReport er, HistoricalData erDay, HistoricalData erDayAfter, List<HistoricalData> checkRange, CancellationToken cToken);
}

public partial class MarketPatternMatcher
{
    public async Task<List<AlertData>> JumpLowCheck(EarningsReport er, HistoricalData erDay, HistoricalData erDayAfter, List<HistoricalData> checkRange, CancellationToken cToken)
    {
        var newAlerts = new List<AlertData>();
        var prevJumpLowAlertDates = er.Alerts
            .Where(x => x.Symbol == er.Symbol)
            .Where(x => x.Type == $"{AlertType.JumpLowAlert}")
            .Select(x => x.Date)
            .ToList();
        var jumpLowDates = checkRange
            .Where(x => x.Date > erDayAfter.Date)
            .Where(x => x.High >= erDay.High)
            .Where(x => x.Close <= erDay.High)
            .ToList();
        var newJumpLowAlerts = jumpLowDates
            .Where(x => !prevJumpLowAlertDates.Contains(x.Date))
            .ToList();
        foreach (var alertDay in newJumpLowAlerts)
        {
            newAlerts.Add(new AlertData
            {
                AlertType = AlertType.JumpLowAlert,
                Symbol = erDay.Symbol,
                Sector = erDay.IndustryInfo?.Sector,
                Industry = erDay.IndustryInfo?.Industry,
                Category = erDay.IndustryInfo?.Category,
                IsWatched = await _db.IsWatched(er.Symbol, cToken),
                Date = alertDay.Date,
                Message = ",Jump low - reach er day's high"
            });
        }

        return newAlerts;
    }
}