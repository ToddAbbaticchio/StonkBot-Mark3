using StonkBot.Data.Entities;
using StonkBot.Data.Enums;
using StonkBot.Extensions;
using StonkBot.MarketPatterns.Models;

namespace StonkBot.MarketPatterns;

public partial interface IMarketPatternMatcher
{
    Task<List<AlertData>> LowHalfCheck(EarningsReport er, HistoricalData erDay, HistoricalData erDayAfter, List<HistoricalData> checkRange, CancellationToken cToken);
}

public partial class MarketPatternMatcher
{
    public async Task<List<AlertData>> LowHalfCheck(EarningsReport er, HistoricalData erDay, HistoricalData erDayAfter, List<HistoricalData> checkRange, CancellationToken cToken)
    {
        var newAlerts = new List<AlertData>();
        if (er.Alerts.Any(x => x.Type == $"{AlertType.LowHalfAlert}"))
            return newAlerts;

        var decX = new[] { erDayAfter.Open, erDayAfter.Close }.Max();
        var decY = new[] { erDay.Open, erDay.Close }.Min();
        var c1 = decY - (decY - decX) / 2;
        //var c2 = decY - (decY - decX) / 3 * 2;

        // LowHalfAlerts
        var prevLowHalfAlertDates = er.Alerts
            .Where(x => x.Symbol == er.Symbol)
            .Where(x => x.Type == $"{AlertType.LowHalfAlert}")
            .Select(x => x.Date)
            .ToList();
        var lowHalfAlertDays = checkRange
            .Where(x => x.Date > erDayAfter.Date)
            .Where(x => x.Close <= decY)
            .Where(x => x.High > c1)
            .ToList();
        var newHalfAlerts = lowHalfAlertDays
            .Where(x => !prevLowHalfAlertDates.Contains(x.Date))
            .OrderBy(x => x.Date)
            .ToList();

        foreach (var alertDay in newHalfAlerts)
        {
            newAlerts.Add(new AlertData
            {
                AlertType = AlertType.LowHalfAlert,
                Symbol = erDay.Symbol,
                Sector = erDay.IndustryInfo?.Sector,
                Industry = erDay.IndustryInfo?.Industry,
                Category = erDay.IndustryInfo?.Category,
                IsWatched = await _db.IsWatched(er.Symbol, cToken),
                Date = alertDay.Date,
                Message = "Jump low - test back 1/2"
            });

            break;
        }

        return newAlerts;
    }
}