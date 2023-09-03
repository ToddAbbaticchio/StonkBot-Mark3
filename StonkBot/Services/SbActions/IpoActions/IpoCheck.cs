using System.Diagnostics;
using Discord;
using Microsoft.EntityFrameworkCore;
using StonkBot.Data.Entities;
using StonkBot.Extensions;
using StonkBot.Extensions.Enums;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.SbActions._Models;
using StonkBot.Services.TDAmeritrade.APIClient.Models;

namespace StonkBot.Services.SbActions;

public partial interface ISbAction
{
    Task IpoCheck(CancellationToken cToken);
    Task IpoCheck(DateTime targetDate, CancellationToken cToken);

    Task CheckSecondPass(DateTime targetDate, CancellationToken cToken);
    
    Task IpoTableHealthCheck(CancellationToken cToken);
}

internal partial class SbAction
{
    public async Task IpoCheck(CancellationToken cToken)
    {
        await IpoCheck(DateTime.Today.SbDate(), cToken);
    }

    public async Task IpoCheck(DateTime targetDate, CancellationToken cToken)
    {
        _con.WriteLog(MessageSeverity.Section, _targetLog, "SbActionRunner.IpoCheck - Starting...");
        var timer = new Stopwatch();
        timer.Start();
        
        await CheckOpeningDay(targetDate, cToken);
        await CheckFirstPass(targetDate, cToken);
        await CheckSecondPass(targetDate, cToken);

        if (targetDate.GetMarketStatus() == MarketStatus.MarketOpen)
        {
            await RecordHData(cToken);
        }
        else
        {
            _con.WriteLog(MessageSeverity.Info, $"MarketStatus is: {targetDate.GetMarketStatus()} - Can't pull EoD numbers when the market isn't open...");
        }
        
        timer.Stop();
        _con.WriteLog(MessageSeverity.Info, _targetLog, $"Elapsed time: [{timer.Elapsed}]");
    }

    internal async Task CheckOpeningDay(DateTime targetDate, CancellationToken cToken)
    {
        try
        {
            if (targetDate != DateTime.Today.SbDate())
            {
                _con.WriteLog(MessageSeverity.Debug, _targetLog, "Skipping 'CheckOpeningDay' because running on a historical date");
                return;
            }
            
            var openedSymbols = await _db.IpoListings
                .Where(x => x.ExpectedListingDate <= targetDate.SbDate())
                .Where(x => x.Open == null)
                .ToListAsync(cToken);

            if (!openedSymbols.Any())
                return;

            var openingSymbols = openedSymbols.Select(x => x.Symbol).ToList();
            var quoteList = await _tdaClient.GetQuotesAsync(openingSymbols, cToken);
            
            foreach (var ipo in openedSymbols)
            {
                if (ipo.ExpectedListingDate < targetDate)
                {
                    // get historical quotes for the last year
                    try
                    {
                        var historicalQuotes = await _tdaClient.GetHistoricalDataAsync(ipo.Symbol, "year", "1", "daily", cToken);
                        if (historicalQuotes == null || !historicalQuotes.candles.Any())
                        {
                            _con.WriteLog(MessageSeverity.Info, _targetLog, $"Unable to retrieve quote history for {ipo.Symbol}! Skipping!");
                            continue;
                        }

                        // if we have the exact day - great.  If not, select the first day we do have and update things
                        var openingDayQuote = historicalQuotes.candles.FirstOrDefault(x => x.GoodDateTime == targetDate);
                        if (openingDayQuote == null)
                        {
                            var actualOpen = historicalQuotes.candles
                                .MinBy(x => x.GoodDateTime);
                            if (actualOpen == null)
                                continue;

                            ipo.ExpectedListingDate = actualOpen.GoodDateTime;
                            ipo.Open = actualOpen.open;
                            ipo.Close = actualOpen.close;
                            ipo.Low = actualOpen.low;
                            ipo.High = actualOpen.high;
                            ipo.Volume = actualOpen.volume;

                            _con.WriteLog(MessageSeverity.Info, _targetLog, $"Looks like {ipo.Symbol} actually opened on {actualOpen.GoodDateTime.SbDateString()}... Updated and added opening day info!");
                            continue;
                        }

                        ipo.Open = openingDayQuote.open;
                        ipo.Close = openingDayQuote.close;
                        ipo.Low = openingDayQuote.low;
                        ipo.High = openingDayQuote.high;
                        ipo.Volume = openingDayQuote.volume;
                        _con.WriteLog(MessageSeverity.Info, $"Added opening day info for watched Ipo:{ipo.Symbol} (from historical quote)");
                        continue;
                    }
                    catch (Exception ex)
                    {
                        _con.WriteLog(MessageSeverity.Warning, _targetLog, $"Symbol: {ipo.Symbol} - {ex.Message}");
                    }
                }

                if (ipo.ExpectedListingDate == targetDate)
                {
                    var quote = quoteList.FirstOrDefault(x => x?.symbol == ipo.Symbol);
                    if (quote == null)
                    {
                        _con.WriteLog(MessageSeverity.Debug, _targetLog, $"Unable to retrieve a quote for {ipo.Symbol}... skipping!");
                        continue;
                    }

                    ipo.Open = quote.openPrice;
                    ipo.Close = quote.regularMarketLastPrice;
                    ipo.Low = quote.lowPrice;
                    ipo.High = quote.highPrice;
                    ipo.Volume = quote.totalVolume;
                    _con.WriteLog(MessageSeverity.Info, $"Added opening day info for watched Ipo:{ipo.Symbol}");
                }
            }

            await _db.SbSaveChangesAsync(cToken);
        }
        catch (Exception ex)
        {
            _con.WriteLog(MessageSeverity.Error, $"Error during StonkBotActions.Ipo.CheckOpeningDay: {ex.Message}");
        }
    }

    internal async Task CheckFirstPass(DateTime targetDate, CancellationToken cToken)
    {
        try
        {
            // Get ipos that have already opened
            var ipoList = await _db.IpoListings
                .Where(x => x.ExpectedListingDate < targetDate)
                .Where(x => x.High != null)
                .Where(x => x.FirstPassDate != targetDate)
                .ToListAsync(cToken);
            
            if (!ipoList.Any())
                return;

            // Get today's quotes for everything in ipoList
            var ipoListQuotes = await _tdaClient.GetQuotesAsync(ipoList.Select(x => x.Symbol).ToList(), cToken);
            var alertList = new List<IpoFirstPassAlert>();
            foreach (var ipo in ipoList)
            {
                var thisQuote = ipoListQuotes.FirstOrDefault(x => x!.symbol == ipo.Symbol);
                if (thisQuote == null)
                {
                    _con.WriteLog(MessageSeverity.Warning, $"Could not retrieve a quote for watched IPO: {ipo.Symbol}");
                    continue;
                }

                // If today CLOSE <= openingday HIGH, skip
                if (thisQuote.regularMarketLastPrice <= ipo.High!)
                    continue;

                var hData = await _db.IpoHData
                    .Where(x => x.Symbol == ipo.Symbol)
                    .Where(x => x.Date < targetDate)
                    .ToListAsync(cToken);
                var daysSatisfied = 1;
                var checkDay = hData.FirstOrDefault(x => x.Date == targetDate.GetPreviousTradeDay());
                while (checkDay != null && checkDay.Close > ipo.High)
                {
                    if (checkDay.Date.GetMarketStatus() == MarketStatus.MarketOpen)
                        daysSatisfied++;

                    checkDay = hData.FirstOrDefault(x => x.Date == checkDay.Date.GetPreviousTradeDay());
                }
                
                // otherwise add to list
                alertList.Add(new IpoFirstPassAlert(ipo.Symbol, thisQuote.regularMarketLastPrice, ipo.High, targetDate, daysSatisfied));
            }

            if (alertList.Any())
                await _discordClient.SendIpoFirstPassAlertsAsync(alertList, cToken);
        }
        catch (Exception ex)
        {
            _con.WriteLog(MessageSeverity.Error, $"Error during StonkBotActions.Ipo.CheckFirstPass: {ex.Message}");
        }
    }

    public async Task CheckSecondPass(DateTime targetDate, CancellationToken cToken)
    {
        try
        {
            var dbIpos = _db.IpoListings;
            var dbIpoHData = _db.IpoHData;
            var alertList = new List<IpoSecondPassAlert>();

            // Get ipos that have already opened
            var ipoList = await dbIpos
                .Where(x => x.ExpectedListingDate < targetDate)
                .Where(x => x.Low != null)
                .Where(x => x.Open != null)
                .Where(x => x.LastSecondPassDate != targetDate)
                .ToListAsync(cToken);
            if (!ipoList.Any())
                return;

            // Get targetDate's quotes for everything in ipoList
            var ipoListQuotes = await _tdaClient.GetQuotesAsync(ipoList.Select(x => x.Symbol).ToList(), cToken);
            foreach (var ipo in ipoList)
            {
                var matches = dbIpoHData
                    .Where(x => x.Symbol == ipo.Symbol)
                    .Where(x => x.Date > ipo.ExpectedListingDate)
                    .Where(x => x.Date < targetDate)
                    .Where(x => x.Close < ipo.Low)
                    .ToList();

                if (!matches.Any())
                    continue;
                var firstMatch = matches.MinBy(x => x.Date);

                var quote = ipoListQuotes.FirstOrDefault(x => x!.symbol == ipo.Symbol);
                if (quote == null)
                {
                    _con.WriteLog(MessageSeverity.Warning, _targetLog, $"Could not retrieve a quote for matched IPO: {ipo.Symbol}");
                    continue;
                }
                if (quote.regularMarketLastPrice <= ipo.Open)
                    continue;

                var hData = await _db.IpoHData
                    .Where(x => x.Symbol == ipo.Symbol)
                    .Where(x => x.Date < targetDate)
                    .ToListAsync(cToken);
                var daysSatisfied = 1;
                var checkDay = hData.SingleOrDefault(x => x.Date == targetDate.GetPreviousTradeDay());
                while (checkDay != null && checkDay.Close > ipo.Open)
                {
                    if (checkDay.Date.GetMarketStatus() == MarketStatus.MarketOpen)
                        daysSatisfied++;

                    checkDay = hData.SingleOrDefault(x => x.Date == checkDay.Date.GetPreviousTradeDay());
                }

                // add any that make it to here to the alert list
                var alert = new IpoSecondPassAlert(ipo.Symbol, firstMatch!.Close, (decimal)ipo.Low!, quote.regularMarketLastPrice, (decimal)ipo.Open!, firstMatch.Date, targetDate, daysSatisfied);
                alertList.Add(alert);
            }

            // If we met alert conditions process the alerts
            if (alertList.Any())
                await _discordClient.SendIpoSecondPassAlertsAsync(alertList, cToken);
        }
        catch (Exception ex)
        {
            _con.WriteLog(MessageSeverity.Error, $"Error during StonkBotActions.Ipo.CheckFirstPass: {ex.Message}");
        }
    }

    internal async Task RecordHData(CancellationToken cToken)
    {
        try
        {
            //await using var _db = new StonkBotDbContext();
            var dbHistData = _db.IpoHData;
            var dbIpoListings = _db.IpoListings;

            var today = DateTime.Today.SbDate();

            // Update targeted stocks daily numbers after market close
            var ipoList = await dbIpoListings
                .Where(x => x.ExpectedListingDate < today)
                .ToListAsync(cToken);

            var ipoCount = ipoList.Count;
            var newDataCount = 0;
            var quoteList = await _tdaClient.GetQuotesAsync(ipoList.Select(x => x.Symbol).ToList(), cToken);
            foreach (var ipo in ipoList)
            {
                try
                {
                    var dbCheck = await dbHistData.FindAsync(new object?[] { ipo.Symbol, today }, cToken);
                    if (dbCheck != null)
                        continue;

                    var quote = quoteList.FirstOrDefault(x => x!.symbol == ipo.Symbol);
                    if (quote == null)
                        continue;

                    ipo.HData ??= new List<IpoHData>();
                    ipo.HData.Add(new IpoHData
                    {
                        Symbol = quote.symbol,
                        Date = today,
                        Open = quote.openPrice,
                        Close = quote.regularMarketLastPrice,
                        Low = quote.lowPrice,
                        High = quote.highPrice,
                        Volume = quote.totalVolume
                    });
                    newDataCount++;
                }
                catch (Exception ex)
                {
                    _con.WriteLog(MessageSeverity.Warning, $"Unable to record IpoHData for {ipo.Symbol} - {ex.Message}");
                }
            }

            if (newDataCount == 0)
                return;

            await _db.SbSaveChangesAsync(cToken);
        }
        catch (Exception ex)
        {
            _con.WriteLog(MessageSeverity.Error, $"Error during StonkBotActions.IpoCheck: {ex.Message}");
        }
    }

    public async Task IpoTableHealthCheck(CancellationToken cToken)
    {
        const int grabSize = 50;

        try
        {
            var symbolsToCheck = await _db.IpoListings
                .Where(x => x.ExpectedListingDate <= DateTime.Now.Date)
                .Where(x => x.Open == null)
                .ToListAsync(cToken);
            if (!symbolsToCheck.Any())
                return;

            var symbolList = symbolsToCheck.Select(x => x.Symbol).ToList();
            for (var i = 0; i < symbolList.Count; i += grabSize)
            {
                var quoteList = new List<Quote?>();
                var subList = symbolList
                    .Skip(i)
                    .Take(grabSize)
                    .ToList();

                try
                {
                    quoteList = await _tdaClient.GetQuotesAsync(subList, cToken);
                }
                catch (Exception ex)
                {
                    _con.WriteLog(MessageSeverity.Warning, _targetLog, $"Oh look: {ex.Message}");
                }

                if (quoteList.Any())
                    continue;

                foreach (var symbol in subList)
                {
                    var soloQuote = await _tdaClient.GetQuoteAsync(symbol, cToken);
                    if (soloQuote != null)
                        continue;

                    var badListing = await _db.IpoListings.SingleOrDefaultAsync(x => x.Symbol == symbol, cToken);
                    if (badListing == null)
                        continue;

                    _db.IpoListings.Remove(badListing);
                    _con.WriteLog(MessageSeverity.Info, _targetLog, $"Bad Ipo Listing: {symbol} removed from db!");
                }

                _con.WriteProgress(i + grabSize, symbolList.Count);
            }

            _con.WriteProgressComplete("Processing Complete!");
            await _db.SbSaveChangesAsync(cToken);
        }
        catch (Exception ex)
        {
            _con.WriteLog(MessageSeverity.Warning, _targetLog, $"Error performing IpoTableHealthCheck: {ex.Message}");
        }
        
        // attempt get multi-quote

        // If fail, loop symbols and find problems

        // remove problems from table

    }
}