using StonkBot.Data.Entities;
using StonkBot.Data.Enums;
using StonkBot.Extensions;
using StonkBot.MarketPatterns.Models;

namespace StonkBot.MarketPatterns;

public partial interface IMarketPatternMatcher
{
    Task<List<AlertData>> LowDayCheck(EarningsReport er, HistoricalData erDay, List<HistoricalData> checkRange, CancellationToken cToken);
}

public partial class MarketPatternMatcher
{
    public async Task<List<AlertData>> LowDayCheck(EarningsReport er, HistoricalData erDay, List<HistoricalData> checkRange, CancellationToken cToken)
    {
        var newAlerts = new List<AlertData>();

        if (er.Alerts.Any(x => x.Type == $"{AlertType.ErLowDate}"))
            return newAlerts;

        var alertDay = checkRange
            .Where(x => x.Date > erDay.Date)
            .Where(x => x.Low <= erDay.Low)
            .Where(x => x.Close > erDay.Low)
            .MinBy(x => x.Date);
        if (alertDay == null)
            return newAlerts;

        newAlerts.Add(new AlertData
        {
            AlertType = AlertType.ErLowDate,
            Symbol = erDay.Symbol,
            Sector = erDay.IndustryInfo?.Sector,
            Industry = erDay.IndustryInfo?.Industry,
            Category = erDay.IndustryInfo?.Category,
            IsWatched = await _db.IsWatched(er.Symbol, cToken),
            Date = alertDay.Date,
            Message = $",Reached ErDate's low! [{erDay.Date.SbDateString()}] {erDay.Low}"
        });

        return newAlerts;
    }
}