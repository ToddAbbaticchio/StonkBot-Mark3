﻿using StonkBot.Data.Entities;
using StonkBot.Data.Enums;
using StonkBot.Extensions;
using StonkBot.MarketPatterns.Models;

namespace StonkBot.MarketPatterns;

public partial interface IMarketPatternMatcher
{
    Task<List<AlertData>> NegativeStartAlertCheck(EarningsReport er, HistoricalData erDay, List<HistoricalData> checkRange, CancellationToken cToken);
}

public partial class MarketPatternMatcher
{
    public async Task<List<AlertData>> NegativeStartAlertCheck(EarningsReport er, HistoricalData erDay, List<HistoricalData> checkRange, CancellationToken cToken)
    {
        var newAlerts = new List<AlertData>();

        var alert1Exists = er.Alerts.Any(x => x.Type == $"{AlertType.NegStartAlert1}");
        var alert2Exists = er.Alerts.Any(x => x.Type == $"{AlertType.NegStartAlert2}");
        if (alert1Exists && alert2Exists)
            return newAlerts;

        if (!alert1Exists)
        {
            var posDays = checkRange.GetPositiveDays();
            if (posDays.Count == 0)
                return newAlerts;

            foreach (var posDay in posDays)
            {
                var expDate = posDay.Date.GetFollowingTradeDay().GetFollowingTradeDay().GetFollowingTradeDay();
                var alertDay = checkRange
                    .Where(x => x.Date > posDay.Date)
                    .Where(x => x.Close < posDay.Low)
                    .Where(x => x.Close < x.Open)
                    .Where(x => x.Date <= expDate)
                    .MinBy(x => x.Date);

                if (alertDay == null)
                    continue;

                newAlerts.Add(new AlertData
                {
                    AlertType = AlertType.NegStartAlert1,
                    Symbol = erDay.Symbol,
                    Sector = erDay.IndustryInfo?.Sector,
                    Industry = erDay.IndustryInfo?.Industry,
                    Category = erDay.IndustryInfo?.Category,
                    IsWatched = await _db.IsWatched(er.Symbol, cToken),
                    Date = alertDay.Date,
                    Message = "1st negative start alert"
                });

                alert1Exists = true;
                break;
            }
        }

        if (alert1Exists && !alert2Exists)
        {
            var dbAlert = er.Alerts.FirstOrDefault(x => x.Type == $"{AlertType.NegStartAlert1}");
            var newAlert = newAlerts.FirstOrDefault(x => x.Type == $"{AlertType.NegStartAlert1}");
            if (dbAlert == null && newAlert == null)
                return newAlerts;
            var alertDate = dbAlert?.Date ?? newAlert!.Date;

            var posDays = checkRange
                .Where(x => x.Date > alertDate)
                .Where(x => x.Close - x.Open > 0)
                .ToList();
            if (posDays.Count == 0)
                return newAlerts;

            foreach (var posDay in posDays)
            {
                var expDate = posDay.Date.GetFollowingTradeDay().GetFollowingTradeDay().GetFollowingTradeDay();
                var alertDay = checkRange
                    .Where(x => x.Date > posDay.Date)
                    .Where(x => x.Close < posDay.Low)
                    .Where(x => x.Close < x.Open)
                    .Where(x => x.Date <= expDate)
                    .MinBy(x => x.Date);
                if (alertDay == null)
                    continue;

                newAlerts.Add(new AlertData
                {
                    AlertType = AlertType.NegStartAlert2,
                    Symbol = erDay.Symbol,
                    Sector = erDay.IndustryInfo?.Sector,
                    Industry = erDay.IndustryInfo?.Industry,
                    Category = erDay.IndustryInfo?.Category,
                    IsWatched = await _db.IsWatched(er.Symbol, cToken),
                    Date = alertDay.Date,
                    Message = "2nd negative start alert"
                });
            }
        }

        return newAlerts;
    }
}