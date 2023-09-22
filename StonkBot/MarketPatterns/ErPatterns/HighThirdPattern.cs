using StonkBot.Data.Entities;
using StonkBot.Data.Enums;
using StonkBot.Extensions;
using StonkBot.MarketPatterns.Models;

namespace StonkBot.MarketPatterns;

public partial interface IMarketPatternMatcher
{
    Task<List<AlertData>> HighThirdCheck(EarningsReport er, HistoricalData erDay, HistoricalData erDayAfter, List<HistoricalData> checkRange, CancellationToken cToken);
}

public partial class MarketPatternMatcher
{
    public async Task<List<AlertData>> HighThirdCheck(EarningsReport er, HistoricalData erDay, HistoricalData erDayAfter, List<HistoricalData> checkRange, CancellationToken cToken)
    {
        var newAlerts = new List<AlertData>();
        var decX = new[] { erDayAfter.Open, erDayAfter.Close }.Min();
        var decY = new[] { erDay.Open, erDay.Close }.Max();
        //var c1 = decY + (decX - decY) / 2;
        var c2 = decY + (decX - decY) / 3 * 2;

        var prevHighThirdAlertDates = er.Alerts
            .Where(x => x.Symbol == er.Symbol)
            .Where(x => x.Type == $"{AlertType.HighThirdAlert}")
            .Select(x => x.Date)
            .ToList();
        var highThirdAlertDays = checkRange
            .Where(x => x.Date > erDayAfter.Date)
            .Where(x => x.Close >= decY)
            .Where(x => x.Low < c2)
            .ToList();
        var newThirdAlerts = highThirdAlertDays
            .Where(x => !prevHighThirdAlertDates.Contains(x.Date))
            .ToList();
        foreach (var alertDay in newThirdAlerts)
        {
            newAlerts.Add(new AlertData
            {
                AlertType = AlertType.HighThirdAlert,
                Symbol = erDay.Symbol,
                Sector = erDay.IndustryInfo?.Sector,
                Industry = erDay.IndustryInfo?.Industry,
                Category = erDay.IndustryInfo?.Category,
                IsWatched = await _db.IsWatched(er.Symbol, cToken),
                Date = alertDay.Date,
                Message = ",Jump high - test back 1/3"
            });
        }

        return newAlerts;
    }
}