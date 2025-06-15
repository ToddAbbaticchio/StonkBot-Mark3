using StonkBot.Data.Entities;
using StonkBot.Data.Enums;
using StonkBot.Extensions;
using StonkBot.MarketPatterns.Models;

namespace StonkBot.MarketPatterns;

public partial interface IMarketPatternMatcher
{
    Task<List<AlertData>> SureStartAlertCheck(EarningsReport er, HistoricalData erDay, List<HistoricalData> checkRange, CancellationToken cToken);
    //Task<bool> SureStartAlertCheck(EarningsReport er, HistoricalData erDay, HistoricalData erDayAfter, List<HistoricalData> checkRange, CancellationToken cToken);
}

public partial class MarketPatternMatcher
{
    public async Task<List<AlertData>> SureStartAlertCheck(EarningsReport er, HistoricalData erDay, List<HistoricalData> checkRange, CancellationToken cToken)
    {
        var newAlerts = new List<AlertData>();

        var alert1Exists = er.Alerts.Any(x => x.Type == $"{AlertType.SureStartAlert1}");
        var alert2Exists = er.Alerts.Any(x => x.Type == $"{AlertType.SureStartAlert2}");
        if (alert1Exists && alert2Exists)
            return newAlerts;

        var sureDay = checkRange
            .Where(x => x.Close >= erDay.High)
            .MinBy(x => x.Date);
        if (sureDay == null)
            return newAlerts;

        List<HistoricalData>? negDays;
        if (!alert1Exists)
        {
            negDays = checkRange.GetNegativeDays()
                .Where(x => x.Date > sureDay.Date)
                .ToList();
            if (negDays.Count == 0)
                return newAlerts;

            foreach (var negDay in negDays)
            {
                var alertDay = checkRange
                    .Where(x => x.Date > negDay.Date)
                    .Where(x => x.Close > negDay.High)
                    .Where(x => x.Close > x.Open)
                    .Where(x => x.Date <= negDay.Date.GetFollowingTradeDay().GetFollowingTradeDay().GetFollowingTradeDay())
                    .MinBy(x => x.Date);
                if (alertDay == null)
                    continue;

                newAlerts.Add(new AlertData
                {
                    AlertType = AlertType.SureStartAlert1,
                    Symbol = erDay.Symbol,
                    Sector = erDay.IndustryInfo?.Sector,
                    Industry = erDay.IndustryInfo?.Industry,
                    Category = erDay.IndustryInfo?.Category,
                    IsWatched = await _db.IsWatched(er.Symbol, cToken),
                    Date = alertDay.Date,
                    Message = "SURE - 1st start alert"
                });

                alert1Exists = true;
                break;
            }
        }

        if (alert1Exists && !alert2Exists)
        {
            var dbAlert = er.Alerts.FirstOrDefault(x => x.Type == $"{AlertType.SureStartAlert1}");
            var newAlert = newAlerts.FirstOrDefault(x => x.Type == $"{AlertType.SureStartAlert1}");
            if (dbAlert == null && newAlert == null)
                return newAlerts;
            var alertDate = dbAlert?.Date ?? newAlert!.Date;

            negDays = checkRange.GetNegativeDays()
                .Where(x => x.Date > alertDate)
                .ToList();
            if (negDays.Count == 0)
                return newAlerts;

            foreach (var negDay in negDays)
            {
                var alertDay = checkRange
                    .Where(x => x.Date > negDay.Date)
                    .Where(x => x.Close > negDay.High)
                    .Where(x => x.Close > x.Open)
                    .Where(x => x.Date <= negDay.Date.GetFollowingTradeDay().GetFollowingTradeDay().GetFollowingTradeDay())
                    .MinBy(x => x.Date);
                if (alertDay == null)
                    continue;
                
                newAlerts.Add(new AlertData
                {
                    AlertType = AlertType.SureStartAlert2,
                    Symbol = erDay.Symbol,
                    Sector = erDay.IndustryInfo?.Sector,
                    Industry = erDay.IndustryInfo?.Industry,
                    Category = erDay.IndustryInfo?.Category,
                    IsWatched = await _db.IsWatched(er.Symbol, cToken),
                    Date = alertDay.Date,
                    Message = "SURE - 2nd start alert"
                });

                break;
            }
        }

        return newAlerts;
    }
}