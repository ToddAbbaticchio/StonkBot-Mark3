using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using StonkBot.Data.Entities;
using StonkBot.Extensions;
using StonkBot.Extensions.Enums;
using StonkBot.Services.ConsoleWriter.Enums;

namespace StonkBot.Services.SbActions;

public partial interface ISbAction
{
    Task UpdateEndOfDayData(CancellationToken cToken);
}

internal partial class SbAction
{
    public async Task UpdateEndOfDayData(CancellationToken cToken)
    {
        _con.WriteLog(MessageSeverity.Section, _targetLog, "SbActionRunner.UpdateMarketData - Starting...");
        var timer = new Stopwatch();
        timer.Start();

        try
        {
            var today = DateTime.Today.SbDate();
            var marketStatus = today.GetMarketStatus();
            if (marketStatus != MarketStatus.MarketOpen)
                return;

            // Update targeted stocks daily numbers after market close
            var symbolList = await _db.IndustryInfo
                .Select(x => x.Symbol)
                .Distinct()
                .ToListAsync(cToken);
            
            var todayData = await _db.HistoricalData
                .Where(x => x.Date == today)
                .ToListAsync(cToken);

            const int grabSize = 250;
            var updatedSymbols = 0;
            var newData = new List<HistoricalData>();
            for (var i = 0; i < symbolList.Count; i += grabSize)
            {
                var subList = symbolList
                    .Skip(i)
                    .Take(grabSize)
                    .ToList();
                var subListQuotes = await _marketClient.GetQuotesAsync(subList, cToken);

                foreach (var quote in subListQuotes!)
                {
                    if (quote == null)
                        continue;

                    var dbEntry = todayData.FirstOrDefault(x => x.Symbol == quote.symbol);
                    if (dbEntry == null)
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
                        updatedSymbols++;
                        continue;
                    }

                    if (dbEntry.Low == quote.lowPrice && dbEntry.High == quote.highPrice)
                        continue;
                    
                    dbEntry.Low = quote.lowPrice;
                    dbEntry.High = quote.highPrice;
                    updatedSymbols++;
                }
                _con.WriteProgress(i + 250, symbolList.Count);
            }
            _con.WriteProgressComplete("Processing Complete!");
            _con.WriteLog(MessageSeverity.Info, _targetLog, updatedSymbols > 0 ? $"Updating low/high prices for {updatedSymbols} db entries!" : "No updates to save!");

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