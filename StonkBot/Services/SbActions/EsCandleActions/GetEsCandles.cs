using System.Diagnostics;
using StonkBot.Data.Entities;
using StonkBot.Extensions.Enums;
using StonkBot.Extensions;
using StonkBot.Services.ConsoleWriter.Enums;

namespace StonkBot.Services.SbActions;

public partial interface ISbAction
{
    Task GetEsCandles(DateTime targetDate, CancellationToken cToken);
}

internal partial class SbAction
{
    public async Task GetEsCandles(DateTime targetDate, CancellationToken cToken)
    {
        var timer = new Stopwatch();
        timer.Start();
        
        try
        {
            var marketStatus = targetDate.GetMarketStatus();
            if (marketStatus != MarketStatus.MarketOpen)
                return;
            
            _con.WriteLog(MessageSeverity.Section, TargetLog.ActionRunner, $"Acquiring ES candle chart for: {targetDate}.");

            var startTime = targetDate.GetPreviousTradeDay().FuturesMarketStartTime();
            var stopTime = targetDate.FuturesMarketEndTime();
            _con.WriteLog(MessageSeverity.Info, TargetLog.ActionRunner, $"Resolved startTime:[{startTime}] and stopTime:[{stopTime}]");

            var apiCandles = await _marketClient.GetCandlesAsync("ES", null, startTime, stopTime, cToken);
            var candleCount = apiCandles.Count;
            if (candleCount == 0)
            {
                _con.WriteLog(MessageSeverity.Warning, TargetLog.ActionRunner, $"No ES candles returned from API for range:[{startTime}] to [{stopTime}]");
                return;
            }
            
            _con.WriteLog(MessageSeverity.Info, $"Acquired {candleCount} ES candles in provided range. Mapping to EsCandles and adding to db...");
            var esCandles = apiCandles.Select(c => new EsCandle
            {
                ChartTime = c!.GoodDateTime,
                Open = c.open,
                Close = c.close,
                Low = c.low,
                High = c.high,
                Volume = c.volume
            }).ToList();

            await _db.EsCandles.AddRangeAsync(esCandles, cToken);
            await _db.SbSaveChangesAsync(cToken);
            timer.Stop();
            _con.WriteLog(MessageSeverity.Stats, $"Done! Elapsed time: [{timer.Elapsed}] Averaging [{Convert.ToSingle(timer.ElapsedMilliseconds) / Convert.ToSingle(candleCount)}]ms per entry updated!");
        }
        catch (Exception ex)
        {
            _con.WriteLog(MessageSeverity.Error, $"Error acquiring ES candles for : {targetDate}: {ex.Message}");
        }
        finally
        {
            timer.Stop();
        }
    }
}
