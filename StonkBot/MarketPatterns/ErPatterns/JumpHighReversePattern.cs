using StonkBot.Data.Entities;
using StonkBot.Data.Enums;
using StonkBot.Extensions;
using StonkBot.MarketPatterns.Models;

namespace StonkBot.MarketPatterns;

public partial interface IMarketPatternMatcher
{
    Task<List<AlertData>> JumpHighReverseCheck(EarningsReport er, HistoricalData erDay, HistoricalData erDayAfter, List<HistoricalData> checkRange, CancellationToken cToken);
}

public partial class MarketPatternMatcher
{
    public async Task<List<AlertData>> JumpHighReverseCheck(EarningsReport er, HistoricalData erDay, HistoricalData erDayAfter, List<HistoricalData> checkRange, CancellationToken cToken)
    {
        var newAlerts = new List<AlertData>();
        var prevJumpHighReverseAlertDates = er.Alerts
            .Where(x => x.Symbol == er.Symbol)
            .Where(x => x.Type == $"{AlertType.JumpHighReverseAlert}")
            .Select(x => x.Date)
            .ToList();
        var jumpHighReverseDates = checkRange
            .Where(x => x.Date > erDayAfter.Date)
            .Where(x => x.Close <= erDay.Low)
            .ToList();
        var newJumpHighReverseAlerts = jumpHighReverseDates
            .Where(x => !prevJumpHighReverseAlertDates.Contains(x.Date))
            .ToList();
        
        foreach (var alertDay in newJumpHighReverseAlerts)
        {
            newAlerts.Add(new AlertData
            {
                AlertType = AlertType.JumpHighReverseAlert,
                Symbol = erDay.Symbol,
                Sector = erDay.IndustryInfo?.Sector,
                Industry = erDay.IndustryInfo?.Industry,
                Category = erDay.IndustryInfo?.Category,
                IsWatched = await er.IsWatched(),
                Date = alertDay.Date,
                Message = ",Jump high - reverse"
            });
        }
        
        return newAlerts;
    }
}