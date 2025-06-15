using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using StonkBot.Data.Entities;
using StonkBot.Extensions;
using StonkBot.Extensions.Enums;
using StonkBot.Services.ConsoleWriter.Enums;

namespace StonkBot.Services.SbActions;

public partial interface ISbAction
{
    Task GetEndOfDayData(CancellationToken cToken);
}

internal partial class SbAction
{
    public async Task GetEndOfDayData(CancellationToken cToken)
    {
        _con.WriteLog(MessageSeverity.Section, _targetLog, "SbActionRunner.GetMarketData - Starting...");
        var timer = new Stopwatch();
        timer.Start();

        try
        {
            var today = DateTime.Today.SbDate();
            var marketStatus = today.GetMarketStatus();
            if (marketStatus != MarketStatus.MarketOpen)
                return;
            
            // Update targeted stocks daily numbers after market close
            var industryInfoSymbolList = await _db.IndustryInfo
                .Select(x => x.Symbol)
                .Distinct()
                .ToListAsync(cToken);
            var symbolCount = industryInfoSymbolList.Count;

            const int grabSize = 250;
            var newData = new List<HistoricalData>();
            for (var i = 0; i < industryInfoSymbolList.Count; i += grabSize)
            {
                var subList = industryInfoSymbolList
                    .Skip(i)
                    .Take(grabSize)
                    .ToList();
                var subListQuotes = await _marketClient.GetQuotesAsync(subList, cToken);
                
                foreach (var quote in subListQuotes!)
                {
                    if (quote == null)
                        continue;

                    var dbCheck = await _db.HistoricalData.FindAsync(new object?[] { quote.symbol, today }, cToken);
                    if (dbCheck != null)
                        continue;

                    try
                    {
                        newData.Add(new HistoricalData
                        {
                            Symbol = quote.symbol!,
                            Date = today,
                            Open = quote.openPrice,
                            Close = quote.regularMarketLastPrice,
                            Low = quote.lowPrice,
                            High = quote.highPrice,
                            Volume = quote.totalVolume
                        });
                    }
                    catch (Exception ex)
                    {
                        _con.WriteLog(MessageSeverity.Error, _targetLog, $"Error creating HistoricalData record for {quote.symbol}: {ex.Message}");
                    }
                }
                _con.WriteProgress(i + 250, symbolCount);
            }
            _con.WriteProgressComplete("Processing Complete!");
            _con.WriteLog(MessageSeverity.Info, _targetLog, newData.Count > 0 ? $"Saving changes for {newData.Count} db entries!" : "No new data to save!");
            
            // Add data to DB
            if (newData.Count != 0)
                await _db.HistoricalData.AddRangeAsync(newData, cToken);

            await _db.SbSaveChangesAsync(cToken);
        }
        catch (Exception ex)
        {
            _con.WriteLog(MessageSeverity.Error, _targetLog, ex.Message);
        }

        timer.Stop();
        _con.WriteLog(MessageSeverity.Info, _targetLog, $"Elapsed time: [{timer.Elapsed}]");
    }
}