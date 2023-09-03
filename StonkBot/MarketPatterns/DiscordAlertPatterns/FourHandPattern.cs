using Microsoft.EntityFrameworkCore;
using StonkBot.Data.Entities;
using StonkBot.Data.Enums;
using StonkBot.Extensions;
using StonkBot.MarketPatterns.Models;
using StonkBot.Services.ConsoleWriter.Enums;

namespace StonkBot.MarketPatterns;

public partial interface IMarketPatternMatcher
{
    Task<AlertData?> FourHandCheck(HistoricalData checkData, CancellationToken cToken);
}

public partial class MarketPatternMatcher
{
    public async Task<AlertData?> FourHandCheck(HistoricalData checkData, CancellationToken cToken)
    {
        try
        {
            HistoricalData? prevFhTargetDay = null;

            var previousDaysData = await _db.HistoricalData
                .Where(x => x.Symbol == checkData.Symbol)
                .Where(x => x.Date < checkData.Date)
                .Include(x => x.CalculatedFields)
                .Where(x => x.CalculatedFields != null)
                .AsSingleQuery()
                .ToListAsync(cToken);
                
            var targetRange = checkData.CalculatedFields!.LastFHTarget!.Split(",");
            if (checkData.CalculatedFields.FHTargetDay == "true")
            {
                prevFhTargetDay ??= previousDaysData
                    .Where(x => x.CalculatedFields!.FHTargetDay == "true")
                    .MaxBy(x => x.Date);
                if (prevFhTargetDay == null)
                    return null;

                targetRange = prevFhTargetDay.CalculatedFields!.LastFHTarget!.Split(",");
            }

            var lowEnd = Convert.ToDecimal(targetRange[0]);
            var highEnd = Convert.ToDecimal(targetRange[1]);
            if (checkData.Low <= lowEnd || checkData.Low >= highEnd)
                return null;

            prevFhTargetDay ??= previousDaysData
                .Where(x => x.CalculatedFields!.FHTargetDay == "true")
                .MaxBy(x => x.Date);
            if (prevFhTargetDay == null)
                return null;

            // if we have a prevTargetDay, check for 'alert days' between then and now.  if we already hit that condition, bail
            var checkPrevAlerts = previousDaysData
                .Where(x => x.Low > lowEnd)
                .Where(x => x.Low < highEnd)
                .Any(x => x.Date > prevFhTargetDay.Date);
            if (checkPrevAlerts)
                return null;

            return new AlertData
            {
                AlertType = AlertType.FourHand,
                Symbol = checkData.Symbol,
                Industry = checkData.IndustryInfo!.Industry,
                Category = checkData.IndustryInfo.Category,
                Sector = checkData.IndustryInfo.Sector,
                Date = checkData.Date,
                Message = $"{checkData.IndustryInfo.Sector},{checkData.IndustryInfo.Industry},{checkData.IndustryInfo.Category},{checkData.Symbol}"
            };
        }
        catch (Exception ex)
        {
            _con.WriteLog(MessageSeverity.Error, TargetLog.ActionRunner, $"Error checking FourHand alert pattern for {checkData.Symbol} on {checkData.Date.SbDateString()}: {ex.Message}");
            return null;
        }
    }
}