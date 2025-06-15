using StonkBot.Data.Entities;
using StonkBot.Data.Enums;
using StonkBot.Extensions;
using StonkBot.MarketPatterns.Models;

namespace StonkBot.MarketPatterns;

public partial interface IMarketPatternMatcher
{
    Task<List<AlertData>> StartAlertCheck(EarningsReport er, HistoricalData erDay, List<HistoricalData> checkRange, CancellationToken cToken);
}

public partial class MarketPatternMatcher
{
    public async Task<List<AlertData>> StartAlertCheck(EarningsReport er, HistoricalData erDay, List<HistoricalData> checkRange, CancellationToken cToken)
    {
        var newAlerts = new List<AlertData>();

        var alert1Exists = er.Alerts.Any(x => x.Type == $"{AlertType.StartAlert1}");
        var alert2Exists = er.Alerts.Any(x => x.Type == $"{AlertType.StartAlert2}");
        if (alert1Exists && alert2Exists)
            return newAlerts;

        if (!alert1Exists)
        {
            var negDays = checkRange.GetNegativeDays();
            if (negDays.Count == 0)
                return newAlerts;

            foreach (var negDay in negDays)
            {
                var expDate = negDay.Date.GetFollowingTradeDay().GetFollowingTradeDay().GetFollowingTradeDay();
                var alertDay = checkRange
                    .Where(x => x.Date > negDay.Date)
                    .Where(x => x.Date <= expDate)
                    .Where(x => x.Close > negDay.High)
                    .Where(x => x.Close > x.Open)
                    .MinBy(x => x.Date);
                if (alertDay == null)
                    continue;

                newAlerts.Add(new AlertData
                {
                    AlertType = AlertType.StartAlert1,
                    Symbol = erDay.Symbol,
                    Sector = erDay.IndustryInfo?.Sector,
                    Industry = erDay.IndustryInfo?.Industry,
                    Category = erDay.IndustryInfo?.Category,
                    IsWatched = await _db.IsWatched(er.Symbol, cToken),
                    Date = alertDay.Date,
                    Message = "1st start alert"
                });

                alert1Exists = true;
                break;
            }
        }

        if (alert1Exists && !alert2Exists)
        {
            var dbAlert = er.Alerts.FirstOrDefault(x => x.Type == $"{AlertType.StartAlert1}");
            var newAlert = newAlerts.FirstOrDefault(x => x.Type == $"{AlertType.StartAlert1}");
            if (dbAlert == null && newAlert == null)
                return newAlerts;
            var alertDate = dbAlert?.Date ?? newAlert!.Date;

            var negDays = checkRange
                .Where(x => x.Date > alertDate)
                .Where(x => x.Close - x.Open < 0)
                .ToList();
            if (negDays.Count == 0)
                return newAlerts;

            foreach (var negDay in negDays)
            {
                var expDate = negDay.Date.GetFollowingTradeDay().GetFollowingTradeDay().GetFollowingTradeDay();
                var alertDay = checkRange
                    .Where(x => x.Date > negDay.Date)
                    .Where(x => x.Close > negDay.High)
                    .Where(x => x.Close > x.Open)
                    .Where(x => x.Date <= expDate)
                    .MinBy(x => x.Date);

                if (alertDay == null)
                    continue;

                newAlerts.Add(new AlertData
                {
                    AlertType = AlertType.StartAlert2,
                    Symbol = erDay.Symbol,
                    Sector = erDay.IndustryInfo?.Sector,
                    Industry = erDay.IndustryInfo?.Industry,
                    Category = erDay.IndustryInfo?.Category,
                    IsWatched = await _db.IsWatched(er.Symbol, cToken),
                    Date = alertDay.Date,
                    Message = "2nd start alert"
                });

                break;
            }
        }

        return newAlerts;
    }
}