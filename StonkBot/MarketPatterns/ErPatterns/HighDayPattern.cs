using StonkBot.Data.Entities;
using StonkBot.Data.Enums;
using StonkBot.Extensions;
using StonkBot.MarketPatterns.Models;

namespace StonkBot.MarketPatterns;

public partial interface IMarketPatternMatcher
{
    Task<List<AlertData>> HighDayCheck(EarningsReport er, HistoricalData erDay, List<HistoricalData> checkRange, CancellationToken cToken);
}

public partial class MarketPatternMatcher
{
    public async Task<List<AlertData>> HighDayCheck(EarningsReport er, HistoricalData erDay, List<HistoricalData> checkRange, CancellationToken cToken)
    {
        var newAlerts = new List<AlertData>();

        var alertDay = checkRange
            .Where(x => x.Date > erDay.Date)
            .Where(x => x.High >= erDay.High)
            .Where(x => x.Close < erDay.High)
            .MinBy(x => x.Date);
        if (alertDay == null)
            return newAlerts;

        newAlerts.Add(new AlertData
        {
            AlertType = AlertType.ErHighDate,
            Symbol = erDay.Symbol,
            Sector = erDay.IndustryInfo?.Sector,
            Industry = erDay.IndustryInfo?.Industry,
            Category = erDay.IndustryInfo?.Category,
            IsWatched = await _db.IsWatched(er.Symbol, cToken),
            Date = alertDay.Date,
            Message = $"Reached ErDate's high! [{erDay.Date.SbDateString()}] {erDay.High}"
        });

        return newAlerts;
    }
}