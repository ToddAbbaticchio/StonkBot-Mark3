﻿using StonkBot.Data.Entities;
using StonkBot.Data.Enums;
using StonkBot.MarketPatterns.Models;

namespace StonkBot.MarketPatterns;

public partial interface IMarketPatternMatcher
{
    Task<List<AlertData>> JumpHighCheck(EarningsReport er, HistoricalData erDay, HistoricalData erDayAfter, List<HistoricalData> checkRange, CancellationToken cToken);
}

public partial class MarketPatternMatcher
{
    public async Task<List<AlertData>> JumpHighCheck(EarningsReport er, HistoricalData erDay, HistoricalData erDayAfter, List<HistoricalData> checkRange, CancellationToken cToken)
    {
        var newAlerts = new List<AlertData>();
        if (er.Alerts.Any(x => x.Type == $"{AlertType.JumpHighAlert}"))
            return newAlerts;

        var prevJumpHighAlertDates = er.Alerts
            .Where(x => x.Symbol == er.Symbol)
            .Where(x => x.Type == $"{AlertType.JumpHighAlert}")
            .Select(x => x.Date)
            .ToList();
        var jumpHighDates = checkRange
            .Where(x => x.Date > erDayAfter.Date)
            .Where(x => x.Low <= erDay.Low)
            .Where(x => x.Close >= erDay.Low)
            .ToList();
        var newJumpHighAlerts = jumpHighDates
            .Where(x => !prevJumpHighAlertDates.Contains(x.Date))
            .OrderBy(x => x.Date)
            .ToList();
        
        foreach (var alertDay in newJumpHighAlerts)
        {
            newAlerts.Add(new AlertData
            {
                AlertType = AlertType.JumpHighAlert,
                Symbol = erDay.Symbol,
                Sector = erDay.IndustryInfo?.Sector,
                Industry = erDay.IndustryInfo?.Industry,
                Category = erDay.IndustryInfo?.Category,
                IsWatched = await _db.IsWatched(er.Symbol, cToken),
                Date = alertDay.Date,
                Message = "Jump high - reach er day's low"
            });

            break;
        }

        return newAlerts;
    }
}