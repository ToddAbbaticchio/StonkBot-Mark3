using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic.FileIO;
using StonkBot.Data.Entities;
using StonkBot.Extensions;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.ConsoleWriter.Models;
using StonkBot.Services.SbActions._Models;

namespace StonkBot.Services.SbActions;

public partial interface ISbAction
{
    Task Csv2DbTable(CancellationToken cToken);
    Task ScanFixMissingPeriod(DateTime startDate, string period, int count, CancellationToken cToken);
}

internal partial class SbAction
{
    public async Task Csv2DbTable(CancellationToken cToken)
    {
        //await using var _db = new StonkBotDbContext();

        var dbTable = _db.EsCandles;
        const string path = @"C:\users\tabba\desktop\testName.csv";

        _con.WriteLog(MessageSeverity.Section, _targetLog, "SbSpecialAction.Csv2DbTable - Starting...");
        var timer = new Stopwatch();
        timer.Start();
        try
        {
            var importList = new List<EsCandle>();

            using var csvParser = new TextFieldParser(path);
            csvParser.CommentTokens = new string[] { "#" };
            csvParser.SetDelimiters(new string[] { "," });
            csvParser.HasFieldsEnclosedInQuotes = true;

            // skip header row
            csvParser.ReadLine();
            while (!csvParser.EndOfData)
            {
                var c = csvParser.ReadFields();
                importList.Add(new EsCandle()
                {
                    Open = Convert.ToDecimal(c![0]),
                    Close = Convert.ToDecimal(c[1]),
                    Low = Convert.ToDecimal(c[2]),
                    High = Convert.ToDecimal(c[3]),
                    Volume = Convert.ToDecimal(c[4]),
                    ChartTime = Convert.ToDateTime(c[5])
                });
            }

            bool EnsureEntityNotExist(EsCandle c) => dbTable.Find(c.ChartTime) == null;
            var cleanList = importList
                .DistinctBy(x => x.ChartTime)
                .Where(EnsureEntityNotExist)
                .ToList();

            await dbTable.AddRangeAsync(cleanList, cToken);
            await _db.SbSaveChangesAsync(cToken);
        }
        catch (Exception ex)
        {
            _con.WriteLog(MessageSeverity.Info, $"breakpoint! {ex.Message}");
        }

        timer.Stop();
        _con.WriteLog(MessageSeverity.Info, _targetLog, $"Done! Elapsed time: [{timer.Elapsed}]");
    }

    public async Task ScanFixMissingPeriod(DateTime startDate, string period, int count, CancellationToken cToken)
    {
        //await using var _db = new StonkBotDbContext();

        _con.WriteLog(MessageSeverity.Info, _targetLog, $"Finding missing info in the last {count} {period}(s)...");
        var hData = _db.HistoricalData;
        var symbolList = await hData.Select(x => x.Symbol)
            .Distinct()
            .ToListAsync(cToken);
        var symbolCount = symbolList.Count;
        var missingData = new List<MissingInfo>();

        var scanCount = 0;
        var prevDays = count;
        if (period == "month")
            prevDays = count * 30;
        if (period == "year")
            prevDays = count * 365;
        
        foreach (var symbol in symbolList)
        {
            try
            {
                var prevDateList = startDate.GetPrevTradeDays(prevDays);
                var dbDateList = await hData
                    .Where(x => x.Symbol == symbol)
                    .Select(x => x.Date)
                    .ToListAsync(cToken);
                var missingDays = prevDateList
                    .Where(x => !dbDateList.Contains(x.Date))
                    .ToList();
                
                missingDays.ForEach(x => missingData.Add(new MissingInfo(symbol, x)));
            }
            catch (Exception ex)
            {
                _con.WriteLog(MessageSeverity.Error, _targetLog, $"Error scanning {symbol} for missing data: {ex.Message}");
            }
            finally
            {
                scanCount++;
                if (scanCount % 100 == 0)
                {
                    _con.WriteProgress(scanCount, symbolCount);
                }

                if (scanCount == symbolCount)
                    _con.WriteProgressComplete("ScanComplete!");
            }
        }

        var missingSymbols = missingData.Select(x => x.Symbol).Distinct().ToList();
        var missingSymbolsCount = missingSymbols.Count;
        _con.WriteLog(MessageSeverity.Info, _targetLog, $"{missingSymbols.Count} Symbols were missing data in the given time period.");

        var processedCount = 0;
        var fixedCount = 0;
        var lastFixedCount = 0;
        var tdaErrList = new List<LogEntry>();
        foreach (var symbol in missingSymbols)
        {
            try
            {
                var goodDataRange = await _tdaClient.GetHistoricalDataAsync(symbol, period, $"{count}", "daily", cToken);
                _con.WriteLog(MessageSeverity.Info, _targetLog, $"HISTORICAL DATA FOR {symbol} GET!!!!");
                var actionItems = missingData.Where(x => x.Symbol == symbol).ToList();
                foreach (var item in actionItems)
                {
                    var matchData = goodDataRange!.candles.FirstOrDefault(x => x.datetime.SbDate() == item.Date);
                    if (matchData == null)
                        continue;

                    await hData.AddAsync(new HistoricalData
                    {
                        Symbol = item.Symbol,
                        Date = matchData.datetime.SbDate(),
                        Open = matchData.open,
                        Close = matchData.close,
                        Low = matchData.low,
                        High = matchData.high,
                        Volume = matchData.volume,
                    }, cToken);
                    fixedCount++;
                }
            }
            catch (Exception ex)
            {
                tdaErrList.Add(new LogEntry(MessageSeverity.Error, TargetLog.ActionRunner, ex.Message));
                //_con.WriteLog(MessageSeverity.Error, _targetLog, $"Error scanning {symbol} for missing data: {ex.Message}");
            }
            finally
            {
                processedCount++;
                _con.WriteProgress("Acquiring Missing Data", processedCount, missingSymbolsCount);
                if (processedCount % 25 == 0)
                {
                    if (fixedCount > lastFixedCount)
                    {
                        await _db.SbSaveChangesAsync(cToken);
                        lastFixedCount = fixedCount;
                    }
                }

                if (processedCount == missingSymbolsCount)
                {
                    _con.WriteProgressComplete("ScanComplete!");
                    
                    if (tdaErrList.Any())
                    {
                        _con.WriteLog(MessageSeverity.Info, _targetLog, "The following errors were encountered while acquiring required historical data:");
                        tdaErrList.ForEach(x => _con.WriteLog(x));
                    }
                }
            }
        }

        if (fixedCount > 0)
        {
            await _db.SbSaveChangesAsync(cToken);
            _con.WriteLog(MessageSeverity.Info, $"Fixed {fixedCount} missing entries in the past {count} {period}(s)!");
            return;
        }

        _con.WriteLog(MessageSeverity.Info, $"No changes were effected for the past {count} {period}(s) data.");
    }

    /*public async Task FixMissingDataTargeted(DateTime targetDate, string period, int count, CancellationToken cToken)
    {
        _con.WriteLog(MessageSeverity.Info, $"Finding missing info from: {targetDate.Date:MM-dd-yyyy}...");
        var hData = _db.HistoricalData;
        var symbolList = await hData.Select(x => x.Symbol)
            .Distinct()
            .ToListAsync(cToken);
        var symbolCount = symbolList.Count;
        var missingData = new List<MissingInfo>();

        var scanCount = 0;
        var prevDays = count;
        if (period == "month")
            prevDays = count * 30;
        if (period == "year")
            prevDays = count * 365;

        foreach (var symbol in symbolList)
        {
            try
            {
                // Check for data on targetDate
                var dataCheck = await hData.FirstOrDefaultAsync(x => x.Date == targetDate.Date, cToken);
                if (dataCheck != null)
                    continue;
                
                missingData.Add(new MissingInfo(symbol, targetDate));
            }
            catch (Exception ex)
            {
                _con.WriteLog(MessageSeverity.Error, $"Error scanning {symbol} for missing data: {ex.Message}");
            }
            finally
            {
                scanCount++;
                if (scanCount % 100 == 0)
                {
                    _con.WriteProgress(scanCount, symbolCount);
                }

                if (scanCount == symbolCount)
                    _con.WriteProgressComplete("ScanComplete!");
            }
        }

        var missingSymbols = missingData.Select(x => x.Symbol).Distinct().ToList();
        var missingSymbolsCount = missingSymbols.Count;
        _con.WriteLog(MessageSeverity.Info, $"{missingSymbols.Count} Symbols were missing data in the given time period.");

        var processedCount = 0;
        var fixedCount = 0;
        var lastFixedCount = 0;
        var tdaErrList = new List<LogEntry>();
        foreach (var symbol in missingSymbols)
        {
            try
            {
                var goodDataRange = await _tdaClient.GetHistoricalDataAsync(symbol, period, $"{count}", "daily", cToken);
                var actionItems = missingData.Where(x => x.Symbol == symbol).ToList();
                foreach (var item in actionItems)
                {
                    var matchData = goodDataRange!.candles.FirstOrDefault(x => x.datetime.SbDate() == item.Date);
                    if (matchData == null)
                        continue;

                    await hData.AddAsync(new HistoricalData
                    {
                        Symbol = item.Symbol,
                        Date = matchData.datetime.SbDate(),
                        Open = matchData.open,
                        Close = matchData.close,
                        Low = matchData.low,
                        High = matchData.high,
                        Volume = matchData.volume,
                    }, cToken);
                    fixedCount++;
                }
            }
            catch (Exception ex)
            {
                tdaErrList.Add(new LogEntry(MessageSeverity.Error, TargetLog.ActionRunner, ex.Message));
                //_con.WriteLog(MessageSeverity.Error, $"Error scanning {symbol} for missing data: {ex.Message}");
            }
            finally
            {
                processedCount++;
                if (processedCount % 25 == 0)
                {
                    _con.WriteProgress("Acquiring Missing Data", processedCount, missingSymbolsCount);

                    if (fixedCount > lastFixedCount)
                    {
                        await _db.SbSaveChangesAsync(cToken);
                        lastFixedCount = fixedCount;
                    }
                }

                if (processedCount == missingSymbolsCount)
                {
                    _con.WriteProgressComplete("ScanComplete!");

                    if (tdaErrList.Any())
                    {
                        _con.WriteLog(MessageSeverity.Info, "The following errors were encountered while acquiring required historical data:");
                        tdaErrList.ForEach(x => _con.WriteLog(x));
                    }
                }
            }
        }

        if (fixedCount > 0)
        {
            await _db.SbSaveChangesAsync(cToken);
            _con.WriteLog(MessageSeverity.Info, $"Fixed {fixedCount} missing entries in the past {count} {period}(s)!");
            return;
        }

        _con.WriteLog(MessageSeverity.Info, $"No changes were effected for the past {count} {period}(s) data.");
    }*/
}