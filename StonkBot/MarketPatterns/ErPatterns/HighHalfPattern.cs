using StonkBot.Data.Entities;
using StonkBot.Data.Enums;
using StonkBot.MarketPatterns.Models;

namespace StonkBot.MarketPatterns;

public partial interface IMarketPatternMatcher
{
    Task<List<AlertData>> HighHalfCheck(EarningsReport er, HistoricalData erDay, HistoricalData erDayAfter, List<HistoricalData> checkRange, CancellationToken cToken);
}

public partial class MarketPatternMatcher
{
    public async Task<List<AlertData>> HighHalfCheck(EarningsReport er, HistoricalData erDay, HistoricalData erDayAfter, List<HistoricalData> checkRange, CancellationToken cToken)
    {
        var newAlerts = new List<AlertData>();
        if (er.Alerts.Any(x => x.Type == $"{AlertType.HighHalfAlert}"))
            return newAlerts;

        var decX = new[] { erDayAfter.Open, erDayAfter.Close }.Min();
        var decY = new[] { erDay.Open, erDay.Close }.Max();
        var c1 = decY + (decX - decY) / 2;
        //var c2 = decY + (decX - decY) / 3 * 2;

        var prevHighHalfAlertDates = er.Alerts
            .Where(x => x.Type == $"{AlertType.HighHalfAlert}")
            .Select(x => x.Date)
            .ToList();
        var highHalfAlertDays = checkRange
            .Where(x => x.Date > erDayAfter.Date)
            .Where(x => x.Close >= decY)
            .Where(x => x.Low < c1)
            .ToList();
        var newHalfAlerts = highHalfAlertDays
            .Where(x => !prevHighHalfAlertDates.Contains(x.Date))
            .OrderBy(x => x.Date)
            .ToList();

        foreach (var alertDay in newHalfAlerts)
        {
            newAlerts.Add(new AlertData
            {
                AlertType = AlertType.HighHalfAlert,
                Symbol = erDay.Symbol,
                Sector = erDay.IndustryInfo?.Sector,
                Industry = erDay.IndustryInfo?.Industry,
                Category = erDay.IndustryInfo?.Category,
                IsWatched = await _db.IsWatched(er.Symbol, cToken),
                Date = alertDay.Date,
                Message = "Jump high - test back 1/2"
            });

            break;
        }

        return newAlerts;
    }
}