using StonkBot.Data.Entities;
using StonkBot.Data.Enums;
using StonkBot.Extensions;
using StonkBot.MarketPatterns.Models;

namespace StonkBot.MarketPatterns;

public partial interface IMarketPatternMatcher
{
    Task<List<AlertData>> SureNegativeStartAlertCheck(EarningsReport er, HistoricalData erDay, List<HistoricalData> checkRange, CancellationToken cToken);
}

public partial class MarketPatternMatcher
{
    public async Task<List<AlertData>> SureNegativeStartAlertCheck(EarningsReport er, HistoricalData erDay, List<HistoricalData> checkRange, CancellationToken cToken)
    {
        var newAlerts = new List<AlertData>();

        var alert1Exists = er.Alerts.Any(x => x.Type == $"{AlertType.SureNegStartAlert1}");
        var alert2Exists = er.Alerts.Any(x => x.Type == $"{AlertType.SureNegStartAlert2}");
        if (alert1Exists && alert2Exists)
            return newAlerts;

        var sureDay = checkRange
            .Where(x => x.Close <= erDay.Low)
            .MinBy(x => x.Date);

        if (sureDay == null)
            return newAlerts;

        if (!alert1Exists)
        {
            var posDays = checkRange
                .Where(x => x.Date > sureDay.Date)
                .Where(x => x.Close - x.Open > 0)
                .ToList();
            if (!posDays.Any())
                return newAlerts;

            foreach (var posDay in posDays)
            {
                var alertDay = checkRange
                    .Where(x => x.Date > posDay.Date)
                    .Where(x => x.Close < posDay.Low)
                    .Where(x => x.Close < x.Open)
                    .Where(x => x.Date <= posDay.Date.GetFollowingTradeDay().GetFollowingTradeDay().GetFollowingTradeDay())
                    .MinBy(x => x.Date);

                if (alertDay == null)
                    continue;

                newAlerts.Add(new AlertData
                {
                    AlertType = AlertType.SureNegStartAlert1,
                    Symbol = erDay.Symbol,
                    Sector = erDay.IndustryInfo?.Sector,
                    Industry = erDay.IndustryInfo?.Industry,
                    Category = erDay.IndustryInfo?.Category,
                    IsWatched = await _db.IsWatched(er.Symbol, cToken),
                    Date = alertDay.Date,
                    Message = "SURE,1st negative start alert"
                });

                alert1Exists = true;
                break;
            }
        }

        if (alert1Exists && !alert2Exists)
        {
            var dbAlert = er.Alerts.FirstOrDefault(x => x.Type == $"{AlertType.SureNegStartAlert1}");
            var newAlert = newAlerts.FirstOrDefault(x => x.Type == $"{AlertType.SureNegStartAlert1}");
            if (dbAlert == null && newAlert == null)
                return newAlerts;
            var alertDate = dbAlert?.Date ?? newAlert!.Date;

            var posDays = checkRange
                .Where(x => x.Date > alertDate)
                .Where(x => x.Close - x.Open > 0)
                .ToList();
            if (!posDays.Any())
                return newAlerts;

            foreach (var posDay in posDays)
            {
                var alertDay = checkRange
                    .Where(x => x.Date > posDay.Date)
                    .Where(x => x.Close < posDay.Low)
                    .Where(x => x.Close < x.Open)
                    .Where(x => x.Date <= posDay.Date.GetFollowingTradeDay().GetFollowingTradeDay().GetFollowingTradeDay())
                    .MinBy(x => x.Date);
                if (alertDay == null)
                    continue;

                newAlerts.Add(new AlertData
                {
                    AlertType = AlertType.SureNegStartAlert2,
                    Symbol = erDay.Symbol,
                    Sector = erDay.IndustryInfo?.Sector,
                    Industry = erDay.IndustryInfo?.Industry,
                    Category = erDay.IndustryInfo?.Category,
                    IsWatched = await _db.IsWatched(er.Symbol, cToken),
                    Date = alertDay.Date,
                    Message = "SURE,2nd negative start alert"
                });

                break;
            }
        }

        return newAlerts;
    }
}