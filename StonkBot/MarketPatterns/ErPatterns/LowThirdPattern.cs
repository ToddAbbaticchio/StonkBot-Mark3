using StonkBot.Data.Entities;
using StonkBot.Data.Enums;
using StonkBot.Extensions;
using StonkBot.MarketPatterns.Models;

namespace StonkBot.MarketPatterns;

public partial interface IMarketPatternMatcher
{
    Task<List<AlertData>> LowThirdCheck(EarningsReport er, HistoricalData erDay, HistoricalData erDayAfter, List<HistoricalData> checkRange, CancellationToken cToken);
}

public partial class MarketPatternMatcher
{
    public async Task<List<AlertData>> LowThirdCheck(EarningsReport er, HistoricalData erDay, HistoricalData erDayAfter, List<HistoricalData> checkRange, CancellationToken cToken)
    {
        var newAlerts = new List<AlertData>();
        if (er.Alerts.Any(x => x.Type == $"{AlertType.LowThirdAlert}"))
            return newAlerts;

        var decX = new[] { erDayAfter.Open, erDayAfter.Close }.Max();
        var decY = new[] { erDay.Open, erDay.Close }.Min();
        //var c1 = decY - (decY - decX) / 2;
        var c2 = decY - (decY - decX) / 3 * 2;

        var prevLowThirdAlertDates = er.Alerts
            .Where(x => x.Symbol == er.Symbol)
            .Where(x => x.Type == $"{AlertType.LowThirdAlert}")
            .Select(x => x.Date)
            .ToList();
        var lowThirdAlertDays = checkRange
            .Where(x => x.Date > erDayAfter.Date)
            .Where(x => x.Close <= decY)
            .Where(x => x.High > c2)
            .ToList();
        var newThirdAlerts = lowThirdAlertDays
            .Where(x => !prevLowThirdAlertDates.Contains(x.Date))
            .OrderBy(x => x.Date)
            .ToList();
        
        foreach (var alertDay in newThirdAlerts)
        {
            newAlerts.Add(new AlertData
            {
                AlertType = AlertType.LowThirdAlert,
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