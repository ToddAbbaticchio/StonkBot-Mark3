/*using System.Collections.Concurrent;
using Discord.Webhook;
using Microsoft.EntityFrameworkCore;
using StonkBot.StonkBot._SBResources;
using StonkBot.StonkBot.Actions.Enums;
using StonkBot.StonkBot.Actions.Models;
using StonkBot.StonkBot.Database;
using StonkBot.StonkBot.Database.Entities.IndustryAnalysis;
using StonkBot.StonkBot.Services.TDAmeritrade;
using StonkBot.StonkBot.Utilities;
using System.Diagnostics;

namespace StonkBot.StonkBot.Actions;
public interface ISpecialActions
{
    Task CalculateField(CalculatedFields field, bool recalculateAll, CancellationToken cToken);
    Task ScanForAlerts(Alerts alert, DateTime targetDate, CancellationToken cToken);

    Task ParallelTestCalculateField(CalculatedFields field, bool recalculateAll, CancellationToken cToken);



    Task ScanFixMissingMonths(int prevDays, CancellationToken cToken);
    Task ScanFixMissingSymbols(CancellationToken cToken);
    Task RemoveIaSymbolsFromDb(string badStonk, CancellationToken cToken);
    Task FindEmptyDbEntries(CancellationToken cToken);
    Task VerifyClosePrices(string targetStonk, CancellationToken cToken);
    Task FullDeleteSpecificDay(CancellationToken cToken);

    Task IsolateScanForAlerts(string symbol, Alerts alert, DateTime targetDate, CancellationToken cToken);
}

internal class SpecialActions : ISpecialActions
{
    private readonly ITdaApiClient _tdaService;
    private readonly StonkBotDbContext _db;
    private readonly ConsoleWriter _cWriter;
    private readonly int _progressTick;

    public SpecialActions(StonkBotDbContext db, ITdaApiClient tdaService, ConsoleWriter cWriter)
    {
        _db = db;
        _tdaService = tdaService;
        _cWriter = cWriter;
        _progressTick = 100;
    }

    public async Task CalculateField(CalculatedFields field, bool recalculateAll, CancellationToken cToken)
    {
        var hData = _db.IAHistoricalData;
        var iaInfo = _db.IAInfo;
        var etfInfo = _db.IAEtfInfo;
        var missingData = field switch
        {
            CalculatedFields.Sector => recalculateAll ? hData : hData.Where(x => string.IsNullOrEmpty(x.Sector)),
            CalculatedFields.Industry => recalculateAll ? hData : hData.Where(x => string.IsNullOrEmpty(x.Industry)),
            CalculatedFields.Category => recalculateAll ? hData : hData.Where(x => string.IsNullOrEmpty(x.Category)),
            CalculatedFields.FromYesterday => recalculateAll ? hData : hData.Where(x => string.IsNullOrEmpty(x.FromYesterday) || x.FromYesterday == "0.0"),
            CalculatedFields.UpToday => recalculateAll ? hData : hData.Where(x => x.UpToday == null),
            CalculatedFields.VolumeAlert => recalculateAll ? hData : hData.Where(x => string.IsNullOrEmpty(x.VolumeAlert)),
            CalculatedFields.VolumeAlert2 => recalculateAll ? hData : hData.Where(x => string.IsNullOrEmpty(x.VolumeAlert2)),
            CalculatedFields.FiveDayStable => recalculateAll ? hData : hData.Where(x => string.IsNullOrEmpty(x.FiveDayStable)),
            CalculatedFields.UpperShadow => recalculateAll ? hData : hData.Where(x => x.UpperShadow == null),
            CalculatedFields.AboveUpperShadow => recalculateAll ? hData : hData.Where(x => x.AboveUpperShadow == null),
            CalculatedFields.FhTargetDay => recalculateAll ? hData : hData.Where(x => x.FHTargetDay == null),
            CalculatedFields.LastFhTarget => recalculateAll ? hData : hData.Where(x => x.LastFHTarget == null && x.FHTargetDay != "true"),
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, null)
        };

        var missingDataCount = missingData.Count();
        _cWriter.WriteMessage(missingDataCount == 0 ? $"No DB entries found missing '{field}' field!" : $"Processing DB entries missing '{field}' field...", ConsoleColor.DarkCyan);
        if (missingDataCount == 0)
            return;

        var updatedEntries = 0;
        var processed = 0;
        var timer = new Stopwatch();
        timer.Start();

        foreach (var entry in missingData)
        {
            try
            {
                switch (field)
                {
                    case CalculatedFields.Sector:
                        {
                            var iaMatch = await iaInfo.FindAsync(new object?[] { entry.Symbol }, cToken);
                            if (iaMatch != null)
                            {
                                entry.Sector = iaMatch.GICSSector.Replace(" ", "").Replace("-", "").Replace(",", "");
                                updatedEntries++;
                                continue;
                            }

                            var etfMatch = await etfInfo.FindAsync(new object?[] { entry.Symbol }, cToken);
                            if (etfMatch != null)
                            {
                                entry.Sector = etfMatch.Sector.Replace(" ", "").Replace("-", "").Replace(",", "");
                                updatedEntries++;
                                continue;
                            }

                            entry.Sector = "missingdata";
                            updatedEntries++;
                            continue;
                        }

                    case CalculatedFields.Industry:
                        {
                            var iaMatch = await iaInfo.FindAsync(new object?[] { entry.Symbol }, cToken);
                            if (iaMatch != null)
                            {
                                entry.Industry = iaMatch.Industry.Replace(" ", "").Replace("-", "").Replace(",", "");
                                updatedEntries++;
                                continue;
                            }

                            var etfMatch = await etfInfo.FindAsync(new object?[] { entry.Symbol }, cToken);
                            if (etfMatch != null)
                            {
                                entry.Industry = "ETF";
                                updatedEntries++;
                                continue;
                            }

                            entry.Industry = "missingdata";
                            updatedEntries++;
                            continue;
                        }

                    case CalculatedFields.Category:
                        {
                            var iaMatch = await iaInfo.FindAsync(new object?[] { entry.Symbol }, cToken);
                            if (iaMatch != null)
                            {
                                entry.Category = iaMatch.Category.Replace(" ", "").Replace("-", "").Replace(",", "");
                                updatedEntries++;
                                continue;
                            }

                            var etfMatch = await etfInfo.FindAsync(new object?[] { entry.Symbol }, cToken);
                            if (etfMatch != null)
                            {
                                entry.Category = "ETF";
                                updatedEntries++;
                                continue;
                            }

                            entry.Category = "missingdata";
                            updatedEntries++;
                            continue;
                        }

                    case CalculatedFields.FromYesterday:
                        {
                            var fyPrevDayDate = TimeHelper.GetPreviousTradeDays(entry.Datetime, 1).FirstOrDefault();
                            var fyPrevDay = await hData.FindAsync(new object?[] { entry.Symbol, fyPrevDayDate }, cToken);
                            if (entry is not { Close: > 0 } || fyPrevDay is not { Close: > 0 })
                            {
                                entry.FromYesterday = "missingdata";
                                updatedEntries++;
                                continue;
                            }
                            var calcPercent = ((entry.Close - fyPrevDay.Close) / fyPrevDay.Close).ToString("P2");
                            entry.FromYesterday = calcPercent;
                            updatedEntries++;
                            continue;
                        }

                    case CalculatedFields.UpToday:
                        {
                            if (entry.Close >= entry.Open)
                            {
                                entry.UpToday = true;
                                updatedEntries++;
                                continue;
                            }
                            entry.UpToday = false;
                            updatedEntries++;
                            continue;
                        }

                    case CalculatedFields.VolumeAlert:
                        {
                            var vaPrevDates = TimeHelper.GetPreviousTradeDays(entry.Datetime, 5);
                            var vaDataList = await hData
                                .Where(x => x.Symbol == entry.Symbol)
                                .Where(x => vaPrevDates.Contains(x.Datetime))
                                .AsSingleQuery()
                                .ToListAsync(cToken);
                            if (vaDataList.Count < 5)
                            {
                                entry.VolumeAlert = "missingdata";
                                updatedEntries++;
                                continue;
                            }

                            var totalVolume = vaDataList.Sum(x => x.Volume);
                            if (totalVolume == 0)
                            {
                                entry.VolumeAlert = "missingdata";
                                updatedEntries++;
                                continue;
                            }

                            var avgVolume = totalVolume / vaDataList.Count;
                            var volPercent = entry.Volume / avgVolume;

                            if (volPercent is >= (decimal)1.2 and <= (decimal)1.6)
                            {
                                entry.VolumeAlert = volPercent.ToString("P2");
                                updatedEntries++;
                                continue;
                            }

                            entry.VolumeAlert = "false";
                            updatedEntries++;
                            continue;
                        }

                    case CalculatedFields.VolumeAlert2:
                        {
                            var va2PrevDates = TimeHelper.GetPreviousTradeDays(entry.Datetime, 5);
                            var va2DataList = await hData
                                .Where(x => x.Symbol == entry.Symbol)
                                .Where(x => va2PrevDates.Contains(x.Datetime))
                                .AsSingleQuery()
                                .ToListAsync(cToken);
                            if (va2DataList.Count < 5)
                            {
                                entry.VolumeAlert2 = "missingdata";
                                updatedEntries++;
                                continue;
                            }

                            var maxVolume = va2DataList.Max(x => x.Volume);
                            if (maxVolume == 0)
                            {
                                entry.VolumeAlert2 = "missingdata";
                                updatedEntries++;
                                continue;
                            }

                            var volPercent2 = entry.Volume / maxVolume;
                            if (volPercent2 is >= (decimal)1.2 and <= (decimal)1.6 && entry.UpToday == true)
                            {
                                entry.VolumeAlert2 = volPercent2.ToString("P2");
                                updatedEntries++;
                                continue;
                            }

                            entry.VolumeAlert2 = "false";
                            updatedEntries++;
                            continue;
                        }

                    case CalculatedFields.FiveDayStable:
                        {
                            var fdPrevDates = TimeHelper.GetPreviousTradeDays(entry.Datetime, 4);
                            var fdDataList = await hData
                                .Where(x => x.Symbol == entry.Symbol)
                                .Where(x => fdPrevDates.Contains(x.Datetime))
                                .AsSingleQuery()
                                .ToListAsync(cToken);
                            if (fdDataList.Count < 5)
                            {
                                entry.FiveDayStable = "missingdata";
                                updatedEntries++;
                                continue;
                            }

                            var combinedList = new List<decimal>();
                            combinedList.AddRange(fdDataList.Select(x => x.Open).ToList());
                            combinedList.AddRange(fdDataList.Select(x => x.Close).ToList());
                            var minVal = combinedList.Min(x => x);
                            var maxVal = combinedList.Max(x => x);

                            if (maxVal <= minVal * (decimal)1.1 && maxVal > minVal * (decimal)1.05)
                            {
                                entry.FiveDayStable = "StrictMatch";
                                updatedEntries++;
                                continue;
                            }

                            if (maxVal <= minVal * (decimal)1.1)
                            {
                                entry.FiveDayStable = "GenerousMatch";
                                updatedEntries++;
                                continue;
                            }

                            entry.FiveDayStable = "nomatch";
                            updatedEntries++;
                            continue;
                        }

                    case CalculatedFields.UpperShadow:
                        {
                            var todayInfo = new List<decimal> { entry.Open, entry.Close, entry.Low, entry.High };
                            var us = todayInfo.OrderBy(x => x).ToArray();
                            if (us[3] - us[2] > us[1] - us[0] && us[2] - us[1] <= (us[3] - us[0]) / (decimal)3)
                            {
                                entry.UpperShadow = true;
                                updatedEntries++;
                                continue;
                            }

                            entry.UpperShadow = false;
                            updatedEntries++;
                            continue;
                        }

                    case CalculatedFields.AboveUpperShadow:
                        {
                            var symbolData = await hData
                                .Where(x => x.Symbol == entry.Symbol)
                                .Where(x => x.Datetime < entry.Datetime)
                                .AsSingleQuery()
                                .ToListAsync(cToken);

                            var lastUpperShadowDay = symbolData
                                .Where(x => x.UpperShadow == true)
                                .MaxBy(x => x.Datetime);

                            if (lastUpperShadowDay == null)
                            {
                                entry.AboveUpperShadow = "false";
                                updatedEntries++;
                                continue;
                            }

                            if (entry.Close > lastUpperShadowDay.High)
                            {
                                var alreadyNotified = symbolData
                                    .Where(x => x.AboveUpperShadow == "firsttrue")
                                    .FirstOrDefault(x => x.Datetime > lastUpperShadowDay.Datetime);

                                if (alreadyNotified == null)
                                {
                                    entry.AboveUpperShadow = "firsttrue";
                                    updatedEntries++;
                                    continue;
                                }

                                entry.AboveUpperShadow = "true";
                                updatedEntries++;
                                continue;
                            }

                            entry.AboveUpperShadow = "false";
                            updatedEntries++;
                            continue;
                        }

                    case CalculatedFields.FhTargetDay:
                        {
                            var fhPrevDates = TimeHelper.GetPreviousTradeDays(entry.Datetime, 3);
                            var fhDataList = await hData
                                .Where(x => x.Symbol == entry.Symbol)
                                .Where(x => fhPrevDates.Contains(x.Datetime))
                                .AsSingleQuery()
                                .ToListAsync(cToken);

                            if (fhDataList.Count != 3)
                            {
                                entry.FHTargetDay = "missingdata";
                                updatedEntries++;
                                continue;
                            }

                            // test on 4 days (today + prev 3)
                            fhDataList.Add(entry);
                            var s = fhDataList.OrderByDescending(x => x.Datetime).ToArray();
                            string? testState = null;
                            for (var i = 0; i <= 3; i++)
                            {
                                if (s[i].Close < s[i].Open)
                                    testState = "failed";

                                if (i < 3 && s[i].Close < s[i + 1].High)
                                    testState = "failed";
                            }

                            if (testState == "failed")
                            {
                                entry.FHTargetDay = "false";
                                updatedEntries++;
                                continue;
                            }

                            entry.FHTargetDay = "true";
                            // dan's new rule
                            if (s[3].Close * (decimal)1.01 <= s[2].Open)
                            {
                                entry.LastFHTarget = $"{s[3].Low},{s[3].High}";
                                updatedEntries++;
                                continue;
                            }
                            // the normal rule
                            entry.LastFHTarget = $"{s[2].Low},{s[2].High}";
                            updatedEntries++;
                            continue;
                        }

                    case CalculatedFields.LastFhTarget:
                        {
                            if (entry.FHTargetDay == "true")
                                continue;

                            var latestRangeDay = await hData
                                .Where(x => x.Symbol == entry.Symbol)
                                .Where(x => x.LastFHTarget != null && x.LastFHTarget != "missingdata")
                                .Where(x => x.Datetime < entry.Datetime)
                                .AsSingleQuery()
                                .OrderBy(x => x.Datetime)
                                .LastOrDefaultAsync(cToken);

                            if (latestRangeDay == null)
                            {
                                entry.LastFHTarget = "missingdata";
                                updatedEntries++;
                                continue;
                            }

                            entry.LastFHTarget = latestRangeDay!.LastFHTarget;
                            updatedEntries++;
                            continue;
                        }

                    default:
                        continue;
                }
            }
            catch (Exception ex)
            {
                _cWriter.WriteMessage($"Error processing {entry.Symbol}-{entry.Datetime} for missing '{field}' field: {ex.Message}");
            }
            finally
            {
                processed++;
                if (processed % _progressTick == 0)
                    _cWriter.WriteProgress(processed, missingDataCount);
                if (processed == missingDataCount)
                    _cWriter.WriteProgressComplete("ProcessingComplete!");
            }
        }

        if (updatedEntries > 0)
        {
            _cWriter.WriteMessage($"Processed {missingDataCount} entries missing '{field}' field. Committing changes to DB...", textColor: ConsoleColor.DarkCyan);
            await _db.SbSaveChangesAsync(cToken);

            timer.Stop();
            _cWriter.WriteMessage($"Done! Elapsed time: [{timer.Elapsed}] Averaging [{Convert.ToSingle(timer.ElapsedMilliseconds) / Convert.ToSingle(updatedEntries)}]ms per entry updated!", textColor: ConsoleColor.DarkCyan);
            return;
        }

        timer.Stop();
        _cWriter.WriteMessage($"Found {missingDataCount} entries missing '{field}' field in [{timer.Elapsed}] but none were fixable.", ConsoleColor.DarkCyan);
    }


    public async Task ParallelTestCalculateField(CalculatedFields field, bool recalculateAll, CancellationToken cToken)
    {
        var hData = _db.IAHistoricalData;
        var iaInfo = _db.IAInfo;
        var etfInfo = _db.IAEtfInfo;
        var missingData = field switch
        {
            CalculatedFields.Sector => recalculateAll ? hData : hData.Where(x => string.IsNullOrEmpty(x.Sector)),
            CalculatedFields.Industry => recalculateAll ? hData : hData.Where(x => string.IsNullOrEmpty(x.Industry)),
            CalculatedFields.Category => recalculateAll ? hData : hData.Where(x => string.IsNullOrEmpty(x.Category)),
            CalculatedFields.FromYesterday => recalculateAll ? hData : hData.Where(x => string.IsNullOrEmpty(x.FromYesterday) || x.FromYesterday == "0.0"),
            CalculatedFields.UpToday => recalculateAll ? hData : hData.Where(x => x.UpToday == null),
            CalculatedFields.VolumeAlert => recalculateAll ? hData : hData.Where(x => string.IsNullOrEmpty(x.VolumeAlert)),
            CalculatedFields.VolumeAlert2 => recalculateAll ? hData : hData.Where(x => string.IsNullOrEmpty(x.VolumeAlert2)),
            CalculatedFields.FiveDayStable => recalculateAll ? hData : hData.Where(x => string.IsNullOrEmpty(x.FiveDayStable)),
            CalculatedFields.UpperShadow => recalculateAll ? hData : hData.Where(x => x.UpperShadow == null),
            CalculatedFields.AboveUpperShadow => recalculateAll ? hData : hData.Where(x => x.AboveUpperShadow == null),
            CalculatedFields.FhTargetDay => recalculateAll ? hData : hData.Where(x => x.FHTargetDay == null),
            CalculatedFields.LastFhTarget => recalculateAll ? hData : hData.Where(x => x.LastFHTarget == null && x.FHTargetDay != "true"),
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, null)
        };

        var missingDataList = missingData.ToList();
        var missingDataCount = missingDataList.Count;
        _cWriter.WriteMessage(missingDataCount == 0 ? $"No DB entries found missing '{field}' field!" : $"Processing DB entries missing '{field}' field...", ConsoleColor.DarkCyan);
        if (missingDataCount == 0)
            return;

        var updatedEntries = 0;
        var processed = 0;
        var timer = new Stopwatch();
        timer.Start();

        var partitioner = Partitioner.Create(missingDataList);
        var partitions = partitioner.GetPartitions(Environment.ProcessorCount);
        var updateBag = new ConcurrentBag<IAHistoricalData>();
        _cWriter.WriteMessage($"Split dataRange into {partitions.Count} partitions...");

        var tasks = partitions.Select(async partition =>
        {
            using (partition)
                while (partition.MoveNext())
                {
                    try
                    {
                        switch (field)
                        {
                            case CalculatedFields.Sector:
                                {
                                    var iaMatch = await iaInfo.FindAsync(new object?[] { partition.Current.Symbol }, cToken);
                                    if (iaMatch != null)
                                    {
                                        partition.Current.Sector = iaMatch.GICSSector.Replace(" ", "").Replace("-", "").Replace(",", "");
                                        updateBag.Add(partition.Current);
                                        updatedEntries++;
                                        continue;
                                    }

                                    var etfMatch = await etfInfo.FindAsync(new object?[] { partition.Current.Symbol }, cToken);
                                    if (etfMatch != null)
                                    {
                                        partition.Current.Sector = etfMatch.Sector.Replace(" ", "").Replace("-", "").Replace(",", "");
                                        updateBag.Add(partition.Current);
                                        updatedEntries++;
                                        continue;
                                    }

                                    partition.Current.Sector = "missingdata";
                                    updateBag.Add(partition.Current);
                                    updatedEntries++;
                                    continue;
                                }

                            case CalculatedFields.Industry:
                                {
                                    var iaMatch = await iaInfo.FindAsync(new object?[] { partition.Current.Symbol }, cToken);
                                    if (iaMatch != null)
                                    {
                                        partition.Current.Industry = iaMatch.Industry.Replace(" ", "").Replace("-", "").Replace(",", "");
                                        updateBag.Add(partition.Current);
                                        updatedEntries++;
                                        continue;
                                    }

                                    var etfMatch = await etfInfo.FindAsync(new object?[] { partition.Current.Symbol }, cToken);
                                    if (etfMatch != null)
                                    {
                                        partition.Current.Industry = "ETF";
                                        updateBag.Add(partition.Current);
                                        updatedEntries++;
                                        continue;
                                    }

                                    partition.Current.Industry = "missingdata";
                                    updateBag.Add(partition.Current);
                                    updatedEntries++;
                                    continue;
                                }

                            case CalculatedFields.Category:
                                {
                                    var iaMatch = await iaInfo.FindAsync(new object?[] { partition.Current.Symbol }, cToken);
                                    if (iaMatch != null)
                                    {
                                        partition.Current.Category = iaMatch.Category.Replace(" ", "").Replace("-", "").Replace(",", "");
                                        updateBag.Add(partition.Current);
                                        updatedEntries++;
                                        continue;
                                    }

                                    var etfMatch = await etfInfo.FindAsync(new object?[] { partition.Current.Symbol }, cToken);
                                    if (etfMatch != null)
                                    {
                                        partition.Current.Category = "ETF";
                                        updateBag.Add(partition.Current);
                                        updatedEntries++;
                                        continue;
                                    }

                                    partition.Current.Category = "missingdata";
                                    updateBag.Add(partition.Current);
                                    updatedEntries++;
                                    continue;
                                }

                            case CalculatedFields.FromYesterday:
                                {
                                    var fyPrevDayDate = TimeHelper.GetPreviousTradeDays(partition.Current.Datetime, 1).FirstOrDefault();
                                    var fyPrevDay = await hData.FindAsync(new object?[] { partition.Current.Symbol, fyPrevDayDate }, cToken);
                                    if (partition.Current is not { Close: > 0 } || fyPrevDay is not { Close: > 0 })
                                    {
                                        partition.Current.FromYesterday = "missingdata";
                                        updateBag.Add(partition.Current);
                                        updatedEntries++;
                                        continue;
                                    }
                                    var calcPercent = ((partition.Current.Close - fyPrevDay.Close) / fyPrevDay.Close).ToString("P2");
                                    partition.Current.FromYesterday = calcPercent;
                                    updateBag.Add(partition.Current);
                                    updatedEntries++;
                                    continue;
                                }

                            case CalculatedFields.UpToday:
                                {
                                    if (partition.Current.Close >= partition.Current.Open)
                                    {
                                        partition.Current.UpToday = true;
                                        updateBag.Add(partition.Current);
                                        updatedEntries++;
                                        continue;
                                    }
                                    partition.Current.UpToday = false;
                                    updateBag.Add(partition.Current);
                                    updatedEntries++;
                                    continue;
                                }

                            case CalculatedFields.VolumeAlert:
                                {
                                    var vaPrevDates = TimeHelper.GetPreviousTradeDays(partition.Current.Datetime, 5);
                                    var vaDataList = await hData
                                        .Where(x => x.Symbol == partition.Current.Symbol)
                                        .Where(x => vaPrevDates.Contains(x.Datetime))
                                        .AsSingleQuery()
                                        .ToListAsync(cToken);
                                    if (vaDataList.Count < 5)
                                    {
                                        partition.Current.VolumeAlert = "missingdata";
                                        updateBag.Add(partition.Current);
                                        updatedEntries++;
                                        continue;
                                    }

                                    var totalVolume = vaDataList.Sum(x => x.Volume);
                                    var avgVolume = totalVolume / vaDataList.Count;
                                    var volPercent = partition.Current.Volume / avgVolume;

                                    if (volPercent is >= (decimal)1.2 and <= (decimal)1.6)
                                    {
                                        partition.Current.VolumeAlert = volPercent.ToString("P2");
                                        updateBag.Add(partition.Current);
                                        updatedEntries++;
                                        continue;
                                    }

                                    partition.Current.VolumeAlert = "false";
                                    updateBag.Add(partition.Current);
                                    updatedEntries++;
                                    continue;
                                }

                            case CalculatedFields.VolumeAlert2:
                                {
                                    var va2PrevDates = TimeHelper.GetPreviousTradeDays(partition.Current.Datetime, 5);
                                    var va2DataList = await hData
                                        .Where(x => x.Symbol == partition.Current.Symbol)
                                        .Where(x => va2PrevDates.Contains(x.Datetime))
                                        .AsSingleQuery()
                                        .ToListAsync(cToken);
                                    if (va2DataList.Count < 5)
                                    {
                                        partition.Current.VolumeAlert2 = "missingdata";
                                        updateBag.Add(partition.Current);
                                        updatedEntries++;
                                        continue;
                                    }

                                    var maxVolume = va2DataList.Max(x => x.Volume);
                                    var volPercent2 = partition.Current.Volume / maxVolume;

                                    if (volPercent2 is >= (decimal)1.2 and <= (decimal)1.6 && partition.Current.UpToday == true)
                                    {
                                        partition.Current.VolumeAlert2 = volPercent2.ToString("P2");
                                        updateBag.Add(partition.Current);
                                        updatedEntries++;
                                        continue;
                                    }

                                    partition.Current.VolumeAlert2 = "false";
                                    updateBag.Add(partition.Current);
                                    updatedEntries++;
                                    continue;
                                }

                            case CalculatedFields.FiveDayStable:
                                {
                                    var fdPrevDates = TimeHelper.GetPreviousTradeDays(partition.Current.Datetime, 4);
                                    var fdDataList = await hData
                                        .Where(x => x.Symbol == partition.Current.Symbol)
                                        .Where(x => fdPrevDates.Contains(x.Datetime))
                                        .AsSingleQuery()
                                        .ToListAsync(cToken);
                                    if (fdDataList.Count < 5)
                                    {
                                        partition.Current.FiveDayStable = "missingdata";
                                        updateBag.Add(partition.Current);
                                        updatedEntries++;
                                        continue;
                                    }

                                    var combinedList = new List<decimal>();
                                    combinedList.AddRange(fdDataList.Select(x => x.Open).ToList());
                                    combinedList.AddRange(fdDataList.Select(x => x.Close).ToList());
                                    var minVal = combinedList.Min(x => x);
                                    var maxVal = combinedList.Max(x => x);

                                    if (maxVal <= minVal * (decimal)1.1 && maxVal > minVal * (decimal)1.05)
                                    {
                                        partition.Current.FiveDayStable = "StrictMatch";
                                        updateBag.Add(partition.Current);
                                        updatedEntries++;
                                        continue;
                                    }

                                    if (maxVal <= minVal * (decimal)1.1)
                                    {
                                        partition.Current.FiveDayStable = "GenerousMatch";
                                        updateBag.Add(partition.Current);
                                        updatedEntries++;
                                        continue;
                                    }

                                    partition.Current.FiveDayStable = "nomatch";
                                    updateBag.Add(partition.Current);
                                    updatedEntries++;
                                    continue;
                                }

                            case CalculatedFields.UpperShadow:
                                {
                                    var todayInfo = new List<decimal> { partition.Current.Open, partition.Current.Close, partition.Current.Low, partition.Current.High };
                                    var us = todayInfo.OrderBy(x => x).ToArray();
                                    if (us[3] - us[2] > us[1] - us[0] && us[2] - us[1] <= (us[3] - us[0]) / (decimal)3)
                                    {
                                        partition.Current.UpperShadow = true;
                                        updateBag.Add(partition.Current);
                                        updatedEntries++;
                                        continue;
                                    }

                                    partition.Current.UpperShadow = false;
                                    updateBag.Add(partition.Current);
                                    updatedEntries++;
                                    continue;
                                }

                            case CalculatedFields.AboveUpperShadow:
                                {
                                    var symbolData = await hData
                                        .Where(x => x.Symbol == partition.Current.Symbol)
                                        .Where(x => x.Datetime < partition.Current.Datetime)
                                        .AsSingleQuery()
                                        .ToListAsync(cToken);

                                    var lastUpperShadowDay = symbolData
                                        .Where(x => x.UpperShadow == true)
                                        .MaxBy(x => x.Datetime);

                                    if (lastUpperShadowDay == null)
                                    {
                                        partition.Current.AboveUpperShadow = "false";
                                        updateBag.Add(partition.Current);
                                        updatedEntries++;
                                        continue;
                                    }

                                    if (partition.Current.Close > lastUpperShadowDay.High)
                                    {
                                        var alreadyNotified = symbolData
                                            .Where(x => x.AboveUpperShadow == "firsttrue")
                                            .FirstOrDefault(x => x.Datetime > lastUpperShadowDay.Datetime);

                                        if (alreadyNotified == null)
                                        {
                                            partition.Current.AboveUpperShadow = "firsttrue";
                                            updateBag.Add(partition.Current);
                                            updatedEntries++;
                                            continue;
                                        }

                                        partition.Current.AboveUpperShadow = "true";
                                        updateBag.Add(partition.Current);
                                        updatedEntries++;
                                        continue;
                                    }

                                    partition.Current.AboveUpperShadow = "false";
                                    updateBag.Add(partition.Current);
                                    updatedEntries++;
                                    continue;
                                }

                            case CalculatedFields.FhTargetDay:
                                {
                                    var fhPrevDates = TimeHelper.GetPreviousTradeDays(partition.Current.Datetime, 3);
                                    var fhDataList = await hData
                                        .Where(x => x.Symbol == partition.Current.Symbol)
                                        .Where(x => fhPrevDates.Contains(x.Datetime))
                                        .AsSingleQuery()
                                        .ToListAsync(cToken);

                                    if (fhDataList.Count != 3)
                                    {
                                        partition.Current.FHTargetDay = "missingdata";
                                        updateBag.Add(partition.Current);
                                        updatedEntries++;
                                        continue;
                                    }

                                    // test on 4 days (today + prev 3)
                                    fhDataList.Add(partition.Current);
                                    var s = fhDataList.OrderByDescending(x => x.Datetime).ToArray();
                                    string? testState = null;
                                    for (var i = 0; i <= 3; i++)
                                    {
                                        if (s[i].Close < s[i].Open)
                                            testState = "failed";

                                        if (i < 3 && s[i].Close < s[i + 1].High)
                                            testState = "failed";
                                    }

                                    if (testState == "failed")
                                    {
                                        partition.Current.FHTargetDay = "false";
                                        updateBag.Add(partition.Current);
                                        updatedEntries++;
                                        continue;
                                    }

                                    partition.Current.FHTargetDay = "true";
                                    // dan's new rule
                                    if (s[3].Close * (decimal)1.01 <= s[2].Open)
                                    {
                                        partition.Current.LastFHTarget = $"{s[3].Low},{s[3].High}";
                                        updatedEntries++;
                                        continue;
                                    }
                                    // the normal rule
                                    partition.Current.LastFHTarget = $"{s[2].Low},{s[2].High}";
                                    updateBag.Add(partition.Current);
                                    updatedEntries++;
                                    continue;
                                }

                            case CalculatedFields.LastFhTarget:
                                {
                                    if (partition.Current.FHTargetDay == "true")
                                        continue;

                                    var latestRangeDay = await hData
                                        .Where(x => x.Symbol == partition.Current.Symbol)
                                        .Where(x => x.LastFHTarget != null && x.LastFHTarget != "missingdata")
                                        .Where(x => x.Datetime < partition.Current.Datetime)
                                        .AsSingleQuery()
                                        .OrderBy(x => x.Datetime)
                                        .LastOrDefaultAsync(cToken);

                                    if (latestRangeDay == null)
                                    {
                                        partition.Current.LastFHTarget = "missingdata";
                                        updateBag.Add(partition.Current);
                                        updatedEntries++;
                                        continue;
                                    }

                                    partition.Current.LastFHTarget = latestRangeDay!.LastFHTarget;
                                    updateBag.Add(partition.Current);
                                    updatedEntries++;
                                    continue;
                                }

                            default:
                                continue;
                        }
                    }
                    catch (Exception ex)
                    {
                        _cWriter.WriteMessage($"Error processing {partition.Current} for missing '{field}' field: {ex.Message}");
                    }
                    finally
                    {
                        processed++;
                        if (processed % _progressTick == 0)
                            _cWriter.WriteProgress(processed, missingDataCount);
                        if (processed == missingDataCount)
                            _cWriter.WriteProgressComplete("ProcessingComplete!");
                    }
                }
        });
        await Task.WhenAll(tasks);

        if (updatedEntries > 0)
        {
            _cWriter.WriteMessage($"Processed {missingDataCount} entries missing '{field}' field. Committing changes to DB...", textColor: ConsoleColor.DarkCyan);
            _db.UpdateRange(updateBag);
            //await _db.SbSaveChangesAsync(cToken);

            timer.Stop();
            _cWriter.WriteMessage($"Done! Elapsed time: [{timer.Elapsed}] Averaging [{Convert.ToSingle(timer.ElapsedMilliseconds) / Convert.ToSingle(updatedEntries)}]ms per entry updated!", textColor: ConsoleColor.DarkCyan);
            return;
        }

        timer.Stop();
        _cWriter.WriteMessage($"Found {missingDataCount} entries missing '{field}' field in [{timer.Elapsed}] but none were fixable.", ConsoleColor.DarkCyan);
    }


    public async Task ScanForAlerts(Alerts alert, DateTime targetDate, CancellationToken cToken)
    {
        targetDate = targetDate.Date;
        var hData = _db.IAHistoricalData;
        var alertData = new List<IAHistoricalData>();

        switch (alert)
        {
            case Alerts.VolumeAlert:
                {
                    alertData = await hData
                        .Where(x => x.Datetime == targetDate)
                        .Where(x => x.UpToday == true)
                        .Where(x => x.VolumeAlert != null)
                        .Where(x => x.VolumeAlert != "missingdata")
                        .Where(x => x.VolumeAlert != "false")
                        .OrderBy(x => x.Sector)
                        .ThenBy(x => x.Industry)
                        .ThenBy(x => x.Category)
                        .AsSingleQuery()
                        .ToListAsync(cToken);

                    if (!alertData.Any())
                        return;
                    break;
                }
            case Alerts.VolumeAlert2:
                {
                    alertData = await hData
                        .Where(x => x.Datetime == targetDate)
                        .Where(x => x.UpToday == true)
                        .Where(x => x.VolumeAlert2 != null)
                        .Where(x => x.VolumeAlert2 != "missingdata")
                        .Where(x => x.VolumeAlert2 != "false")
                        .OrderBy(x => x.Sector)
                        .ThenBy(x => x.Industry)
                        .ThenBy(x => x.Category)
                        .AsSingleQuery()
                        .ToListAsync(cToken);

                    if (!alertData.Any())
                        return;
                    break;
                }
            case Alerts.UpperShadowAlert:
                {
                    alertData = await hData
                        .Where(x => x.Datetime == targetDate)
                        .Where(x => x.AboveUpperShadow == "firsttrue")
                        .OrderBy(x => x.Sector)
                        .ThenBy(x => x.Industry)
                        .ThenBy(x => x.Category)
                        .AsSingleQuery()
                        .ToListAsync(cToken);

                    if (!alertData.Any())
                        return;
                    break;
                }
            case Alerts.FourHandAlert:
                {
                    var dumb = await hData
                        .Where(x => x.Datetime == targetDate)
                        .ToListAsync(cToken);

                    alertData = dumb
                        .Where(x => x.LastFHTarget != null)
                        .Where(x => x.LastFHTarget!.Contains(','))
                        .OrderBy(x => x.Sector)
                        .ThenBy(x => x.Industry)
                        .ThenBy(x => x.Category)
                        .ToList();

                    if (!alertData.Any())
                        return;
                    break;
                }
            case Alerts.IpoAlert:
                {
                    alertData = await hData
                        .Where(x => x.Datetime == targetDate)
                        .OrderBy(x => x.Sector)
                        .ThenBy(x => x.Industry)
                        .ThenBy(x => x.Category)
                        .AsSingleQuery()
                        .ToListAsync(cToken);

                    if (!alertData.Any())
                        return;
                    break;
                }
        }

        var alertDataCount = alertData!.Count;
        _cWriter.WriteMessage(alertDataCount == 0 ? $"No DB entries found matching criteria for {alert} on {targetDate}" : $"Processing DB entries for {alert} alerts on {targetDate}...", ConsoleColor.DarkCyan);
        if (alertDataCount == 0)
            return;

        var tempFile = new List<string>();
        switch (alert)
        {
            case Alerts.VolumeAlert:
                {
                    const string fName = "C:/temp/VolumeAlerts.csv";
                    var volAlert = new DiscordWebhookClient(DiscordWebHook.VolumeAlert);
                    tempFile.Add("Sector,Industry,Category,Symbol,VolPercent");
                    tempFile.AddRange(alertData.Select(entry => $"{entry.Sector},{entry.Industry},{entry.Category},{entry.Symbol},{entry.VolumeAlert}"));
                    await File.WriteAllLinesAsync(fName, tempFile, cToken);
                    var messageId = await volAlert.SendFileAsync(fName, $"VolumeAlerts - Date: [{targetDate:MM/dd/yyyy}]");
                    break;
                }

            case Alerts.VolumeAlert2:
                {
                    const string fName = "C:/temp/VolumeAlerts2.csv";
                    var vol2Alert = new DiscordWebhookClient(DiscordWebHook.VolumeAlert);
                    tempFile.Add("Sector,Industry,Category,Symbol,VolPercent2");
                    tempFile.AddRange(alertData.Select(entry => $"{entry.Sector},{entry.Industry},{entry.Category},{entry.Symbol},{entry.VolumeAlert2}"));
                    await File.WriteAllLinesAsync(fName, tempFile, cToken);
                    var messageId = await vol2Alert.SendFileAsync(fName, $"VolumeAlerts2 - Date: [{targetDate:MM/dd/yyyy}]");
                    break;
                }

            case Alerts.UpperShadowAlert:
                {
                    const string fName = "C:/temp/AboveUpperShadowAlerts.csv";
                    var usAlert = new DiscordWebhookClient(DiscordWebHook.UpperShadow);
                    tempFile.Add("Sector,Industry,Category,Symbol");
                    tempFile.AddRange(alertData.Select(entry => $"{entry.Sector},{entry.Industry},{entry.Category},{entry.Symbol}"));
                    await File.WriteAllLinesAsync(fName, tempFile, cToken);
                    var messageId = await usAlert.SendFileAsync(fName, $"AboveUpperShadowAlerts - Date: [{targetDate:MM/dd/yyyy}]");
                    break;
                }

            case Alerts.FourHandAlert:
                {
                    const string fName = "C:/temp/FourHandAlerts.csv";
                    var fhAlert = new DiscordWebhookClient(DiscordWebHook.FourHand);

                    foreach (var entry in alertData)
                    {
                        var entryRange = await hData
                            .Where(x => x.Symbol == entry.Symbol)
                            .Where(x => x.Datetime < entry.Datetime)
                            .AsSingleQuery()
                            .ToListAsync(cToken);

                        IAHistoricalData? prevFHTargetDay = null;
                        var targetRange = entry.LastFHTarget!.Split(",");

                        if (entry.FHTargetDay == "true")
                        {
                            prevFHTargetDay ??= entryRange
                                .Where(x => x.FHTargetDay == "true")
                                .MaxBy(x => x.Datetime);
                            if (prevFHTargetDay == null)
                                continue;

                            targetRange = prevFHTargetDay.LastFHTarget!.Split(",");
                        }

                        var lowEnd = Convert.ToDecimal(targetRange[0]);
                        var highEnd = Convert.ToDecimal(targetRange[1]);
                        if (entry.Low <= lowEnd || entry.Low >= highEnd)
                            continue;

                        prevFHTargetDay ??= entryRange
                            .Where(x => x.FHTargetDay == "true")
                            .MaxBy(x => x.Datetime);

                        // if we have a prevTargetDay, check for 'alert days' between then and now.  if we already hit tha condition, bail
                        if (prevFHTargetDay != null)
                        {
                            var checkPrevAlerts = entryRange
                                .Where(x => x.Low > lowEnd)
                                .Where(x => x.Low < highEnd)
                                .Any(x => x.Datetime > prevFHTargetDay.Datetime);
                            if (checkPrevAlerts)
                                continue;
                        }

                        if (tempFile.Count == 0)
                            tempFile.Add("Sector,Industry,Category,Symbol");

                        tempFile.Add($"{entry.Sector},{entry.Industry},{entry.Category},{entry.Symbol}");
                    }

                    if (tempFile.Count == 0)
                        break;

                    await File.WriteAllLinesAsync(fName, tempFile, cToken);
                    var messageId = await fhAlert.SendFileAsync(fName, $"FourHandAlerts - Date: [{targetDate:MM/dd/yyyy}]");
                    break;
                }

            case Alerts.IpoAlert:
                {
                    break;
                }
        }
    }

    #region utility methods ------------------------------------------------------------------------------
    public async Task ScanFixMissingMonths(int prevMonths, CancellationToken cToken)
    {
        _cWriter.WriteMessage($"Finding missing info in the last {prevMonths} month(s)...", ConsoleColor.DarkCyan);
        var hData = _db.IAHistoricalData;
        var symbolList = hData.Select(x => x.Symbol).Distinct().ToList();
        var symbolCount = symbolList.Count;
        var missingData = new List<MissingInfo>();

        var scanCount = 0;
        foreach (var symbol in symbolList)
        {
            try
            {
                var prevDateList = TimeHelper.GetPreviousTradeDays(DateTime.Today.Date, prevMonths * 30);
                foreach (var prevDate in prevDateList)
                {
                    var prevData = await hData.FindAsync(new object?[] { symbol, prevDate }, cToken);
                    if (prevData != null)
                        continue;

                    var thisInfo = new MissingInfo
                    {
                        Symbol = symbol,
                        Date = prevDate
                    };
                    missingData.Add(thisInfo);
                }
            }
            catch (Exception ex)
            {
                _cWriter.WriteMessage($"Error scanning {symbol} for missing data: {ex.Message}", ConsoleColor.Red);
            }
            finally
            {
                scanCount++;
                if (scanCount % 100 == 0)
                {
                    _cWriter.WriteProgress(scanCount, symbolCount);
                }

                if (scanCount == symbolCount)
                    _cWriter.WriteProgressComplete("ScanComplete!");
            }
        }

        var missingSymbols = missingData.Select(x => x.Symbol).Distinct().ToList();
        var missingSymbolsCount = missingSymbols.Count;
        _cWriter.WriteMessage($"{missingSymbols.Count} Symbols were missing data in the given time period.", ConsoleColor.DarkCyan);

        var processedCount = 0;
        var fixedCount = 0;
        var lastFixedCount = 0;
        foreach (var symbol in missingSymbols)
        {
            try
            {
                var goodDataRange = await _tdaService.GetHistoricalDataAsync(symbol, "month", $"{prevMonths}", "daily", cToken);
                if (goodDataRange == null)
                    continue;

                goodDataRange.candles.ForEach(x => x.goodDateTime = TimeHelper.ConvertLongToUTCDateTime(x.datetime));

                var actionItems = missingData.Where(x => x.Symbol == symbol).ToList();
                foreach (var thisHistoricalData in from actionItem in actionItems
                                                   let matchData = goodDataRange.candles.FirstOrDefault(x => x.goodDateTime == actionItem.Date)
                                                   where matchData != null
                                                   select new IAHistoricalData
                                                   {
                                                       Symbol = actionItem.Symbol,
                                                       Open = matchData.open,
                                                       Close = matchData.close,
                                                       Low = matchData.low,
                                                       High = matchData.high,
                                                       Volume = matchData.volume,
                                                       Datetime = (DateTime)matchData.goodDateTime!
                                                   })
                {
                    hData.Add(thisHistoricalData);
                    fixedCount++;
                }
            }
            catch (Exception ex)
            {
                _cWriter.WriteMessage($"Error scanning {symbol} for missing data: {ex.Message}", ConsoleColor.Red);
            }
            finally
            {
                processedCount++;
                if (processedCount % 100 == 0)
                {
                    _cWriter.WriteProgress(processedCount, missingSymbolsCount);

                    if (fixedCount > lastFixedCount)
                    {
                        await _db.SbSaveChangesAsync(cToken);
                        lastFixedCount = fixedCount;
                    }
                }

                if (processedCount == missingSymbolsCount)
                    _cWriter.WriteProgressComplete("ScanComplete!");
            }
        }

        if (fixedCount > 0)
        {
            await _db.SbSaveChangesAsync(cToken);
            _cWriter.WriteMessage($"Fixed {fixedCount} missing entries in the past {prevMonths} months!", ConsoleColor.DarkCyan);
            return;
        }

        _cWriter.WriteMessage($"No changes were effected for the past {prevMonths} month(s) data.", ConsoleColor.DarkCyan);
    }

    public async Task ScanFixMissingSymbols(CancellationToken cToken)
    {
        var symbolList = _db.IAInfo.Select(x => x.Symbol).ToList();
        symbolList.AddRange(_db.IAEtfInfo.Select(x => x.Symbol).ToList());

        var hData = _db.IAHistoricalData;
        var hSymbolList = hData.Select(x => x.Symbol).ToList();

        var noDataSymbols = new List<string>();
        var count = 1;
        foreach (var symbol in symbolList)
        {
            if (hSymbolList.Contains(symbol))
            {
                //Console.WriteLine($"HistoricalData already contains entries for {symbol}... skipped!");
                count++;
                continue;
            }

            Console.WriteLine($"Processing {symbol} [{count}] of [{symbolList.Count}]");
            var yearData = await _tdaService.GetHistoricalDataAsync(symbol, "year", "1", "daily", cToken);
            var months = 12;
            while (yearData != null && !yearData.candles.Any() && months > 0)
            {
                yearData = await _tdaService.GetHistoricalDataAsync(symbol, "month", $"{months}", "daily", cToken);
                months--;
            }
            if (!yearData!.candles.Any())
            {
                Console.WriteLine($"No data available for {symbol} even after trying individual months, 12 - 1. Skipping!");
                noDataSymbols.Add(symbol);
                continue;
            }

            foreach (var todayInfo in yearData.candles.Select(day => new IAHistoricalData
            {
                Symbol = yearData.symbol,
                Open = Convert.ToDecimal(day.open),
                Close = Convert.ToDecimal(day.close),
                Low = Convert.ToDecimal(day.low),
                High = Convert.ToDecimal(day.high),
                Volume = Convert.ToDecimal(day.volume),
                Datetime = TimeHelper.ConvertLongToUTCDateTime(day.datetime),
            }))
            {
                await _db.AddAsync(todayInfo, cToken);
            }

            await _db.SbSaveChangesAsync(cToken);
            count++;
        }

        Console.WriteLine($"Fixed this many bullshits: {count}");
        Console.WriteLine($"The following symbols don't seem to have data available: {string.Join($"{Environment.NewLine}  -", noDataSymbols)}");
    }

    public async Task RemoveIaSymbolsFromDb(string badStonk, CancellationToken cToken)
    {
        var hData = _db.IAHistoricalData;
        var iaInfos = _db.IAInfo;
        var iaEtfInfos = _db.IAEtfInfo;

        var iaInfoToRemove = iaInfos
            .FirstOrDefault(x => x.Symbol == badStonk);
        var iaEtfInfoToRemove = iaEtfInfos
            .FirstOrDefault(x => x.Symbol == badStonk);
        var hDataToRemove = await hData
            .Where(x => x.Symbol == badStonk)
            .ToListAsync(cToken);

        if (iaInfoToRemove != null)
            iaInfos.Remove(iaInfoToRemove);
        if (iaEtfInfoToRemove != null)
            iaEtfInfos.Remove(iaEtfInfoToRemove);
        if (hDataToRemove.Count > 0)
            hData.RemoveRange(hDataToRemove);

        await _db.SbSaveChangesAsync(cToken);
    }

    public async Task FindEmptyDbEntries(CancellationToken cToken)
    {
        _cWriter.WriteMessage($"Finding empty DB entries...", ConsoleColor.DarkCyan);
        var hData = _db.IAHistoricalData;
        var bad = (decimal)0;
        var badEntries = hData.Where(x => x.Open == bad && x.Close == bad && x.Low == bad && x.High == bad && x.Volume == bad);

        var fixedCount = 0;
        foreach (var entry in badEntries)
        {
            hData.Remove(entry);
            fixedCount++;
        }

        if (fixedCount > 0)
        {
            await _db.SbSaveChangesAsync(cToken);
            _cWriter.WriteMessage($"Fixed {fixedCount} empty db entries!", ConsoleColor.DarkCyan);
            return;
        }

        _cWriter.WriteMessage($"No empty DB entries were found, no changes were effected.", ConsoleColor.DarkCyan);
    }

    public async Task FullDeleteSpecificDay(CancellationToken cToken)
    {
        var hData = _db.IAHistoricalData;
        var targetDate = DateTime.Parse("2023-01-19 00:00:00").Date;
        var toDelete = hData.Where(x => x.Datetime == targetDate);
        _cWriter.WriteMessage($"Deleting all data for {targetDate}");
        await toDelete.ExecuteDeleteAsync(cToken);
        await _db.SbSaveChangesAsync(cToken);
    }

    public async Task VerifyClosePrices(string targetStonk, CancellationToken cToken)
    {
        var hData = _db.IAHistoricalData;
        var checkData = await _tdaService.GetHistoricalDataAsync(targetStonk, "month", "1", "daily", cToken);
        checkData?.candles.ForEach(x => x.goodDateTime = TimeHelper.ConvertLongToUTCDateTime(x.datetime));

        var badDataList = new List<string>();
        if (checkData?.candles != null)
            foreach (var day in checkData!.candles)
            {
                var dbDay = await hData.FindAsync(new object[] { targetStonk, day.goodDateTime! }, cToken);
                if (dbDay == null)
                {
                    _cWriter.WriteMessage($"No DB data for {day.goodDateTime}", ConsoleColor.DarkYellow);
                    continue;
                }

                if (dbDay.Close == day.close)
                    continue;

                badDataList.Add(day.goodDateTime.ToString()!);
            }

        if (badDataList.Any())
            _cWriter.WriteMessage($"{targetStonk} bad close price days: {string.Join(",", badDataList)}", ConsoleColor.DarkYellow);
        else
            _cWriter.WriteMessage($"{targetStonk} no close price mismatches in the last month!", ConsoleColor.DarkGreen);
    }
    #endregion

    public async Task IsolateScanForAlerts(string symbol, Alerts alert, DateTime targetDate, CancellationToken cToken)
    {
        targetDate = targetDate.Date;
        var hData = _db.IAHistoricalData;
        var alertData = new List<IAHistoricalData>();

        switch (alert)
        {
            case Alerts.VolumeAlert:
                {
                    alertData = await hData
                        .Where(x => x.Symbol == symbol)
                        .Where(x => x.Datetime == targetDate)
                        .Where(x => x.UpToday == true)
                        .Where(x => x.VolumeAlert != null && x.VolumeAlert.Contains('%'))
                        .OrderBy(x => x.Sector)
                        .ThenBy(x => x.Industry)
                        .ThenBy(x => x.Category)
                        .AsSingleQuery()
                        .ToListAsync(cToken);

                    if (!alertData.Any())
                        return;
                    break;
                }
            case Alerts.VolumeAlert2:
                {
                    alertData = await hData
                        .Where(x => x.Symbol == symbol)
                        .Where(x => x.Datetime == targetDate)
                        .Where(x => x.UpToday == true)
                        .Where(x => x.VolumeAlert2 != null && x.VolumeAlert2.Contains('%'))
                        .OrderBy(x => x.Sector)
                        .ThenBy(x => x.Industry)
                        .ThenBy(x => x.Category)
                        .AsSingleQuery()
                        .ToListAsync(cToken);

                    if (!alertData.Any())
                        return;
                    break;
                }
            case Alerts.UpperShadowAlert:
                {
                    alertData = await hData
                        .Where(x => x.Symbol == symbol)
                        .Where(x => x.Datetime == targetDate)
                        .Where(x => x.AboveUpperShadow == "firsttrue")
                        .OrderBy(x => x.Sector)
                        .ThenBy(x => x.Industry)
                        .ThenBy(x => x.Category)
                        .AsSingleQuery()
                        .ToListAsync(cToken);

                    if (!alertData.Any())
                        return;
                    break;
                }
            case Alerts.FourHandAlert:
                {
                    var dumb = await hData
                        .Where(x => x.Symbol == symbol)
                        .Where(x => x.Datetime == targetDate)
                        .ToListAsync(cToken);

                    alertData = dumb
                        .Where(x => x.LastFHTarget != null)
                        .Where(x => x.LastFHTarget!.Contains(','))
                        .OrderBy(x => x.Sector)
                        .ThenBy(x => x.Industry)
                        .ThenBy(x => x.Category)
                        .ToList();

                    if (!alertData.Any())
                        return;
                    break;
                }
            case Alerts.IpoAlert:
                {
                    alertData = await hData
                        .Where(x => x.Symbol == symbol)
                        .Where(x => x.Datetime == targetDate)
                        .OrderBy(x => x.Sector)
                        .ThenBy(x => x.Industry)
                        .ThenBy(x => x.Category)
                        .AsSingleQuery()
                        .ToListAsync(cToken);

                    if (!alertData.Any())
                        return;
                    break;
                }
        }

        var alertDataCount = alertData!.Count;
        _cWriter.WriteMessage(alertDataCount == 0 ? $"No DB entries found matching criteria for {alert} on {targetDate}" : $"Processing DB entries for {alert} alerts on {targetDate}...", ConsoleColor.DarkCyan);
        if (alertDataCount == 0)
            return;

        var tempFile = new List<string>();
        switch (alert)
        {
            case Alerts.VolumeAlert:
                {
                    const string fName = "C:/temp/VolumeAlerts.csv";
                    var volAlert = new DiscordWebhookClient(DiscordWebHook.VolumeAlert);
                    tempFile.Add("Sector,Industry,Category,Symbol,VolPercent");
                    tempFile.AddRange(alertData.Select(entry => $"{entry.Sector},{entry.Industry},{entry.Category},{entry.Symbol},{entry.VolumeAlert}"));
                    await File.WriteAllLinesAsync(fName, tempFile, cToken);
                    var messageId = await volAlert.SendFileAsync(fName, $"VolumeAlerts - Date: [{targetDate:MM/dd/yyyy}]");
                    break;
                }

            case Alerts.VolumeAlert2:
                {
                    const string fName = "C:/temp/VolumeAlerts2.csv";
                    var vol2Alert = new DiscordWebhookClient(DiscordWebHook.VolumeAlert);
                    tempFile.Add("Sector,Industry,Category,Symbol,VolPercent2");
                    tempFile.AddRange(alertData.Select(entry => $"{entry.Sector},{entry.Industry},{entry.Category},{entry.Symbol},{entry.VolumeAlert2}"));
                    await File.WriteAllLinesAsync(fName, tempFile, cToken);
                    var messageId = await vol2Alert.SendFileAsync(fName, $"VolumeAlerts2 - Date: [{targetDate:MM/dd/yyyy}]");
                    break;
                }

            case Alerts.UpperShadowAlert:
                {
                    const string fName = "C:/temp/AboveUpperShadowAlerts.csv";
                    var usAlert = new DiscordWebhookClient(DiscordWebHook.UpperShadow);
                    tempFile.Add("Sector,Industry,Category,Symbol");
                    tempFile.AddRange(alertData.Select(entry => $"{entry.Sector},{entry.Industry},{entry.Category},{entry.Symbol}"));
                    await File.WriteAllLinesAsync(fName, tempFile, cToken);
                    var messageId = await usAlert.SendFileAsync(fName, $"AboveUpperShadowAlerts - Date: [{targetDate:MM/dd/yyyy}]");
                    break;
                }

            case Alerts.FourHandAlert:
                {
                    const string fName = "C:/temp/FourHandAlerts.csv";
                    var fhAlert = new DiscordWebhookClient(DiscordWebHook.FourHand);

                    foreach (var entry in alertData)
                    {
                        var entryRange = await hData
                            .Where(x => x.Symbol == entry.Symbol)
                            .Where(x => x.Datetime < entry.Datetime)
                            .AsSingleQuery()
                            .ToListAsync(cToken);

                        IAHistoricalData? prevFHTargetDay = null;
                        var targetRange = entry.LastFHTarget!.Split(",");

                        if (entry.FHTargetDay == "true")
                        {
                            prevFHTargetDay ??= entryRange
                                .Where(x => x.FHTargetDay == "true")
                                .MaxBy(x => x.Datetime);
                            if (prevFHTargetDay == null)
                                continue;

                            targetRange = prevFHTargetDay.LastFHTarget!.Split(",");
                        }

                        var lowEnd = Convert.ToDecimal(targetRange[0]);
                        var highEnd = Convert.ToDecimal(targetRange[1]);
                        if (entry.Low <= lowEnd || entry.Low >= highEnd)
                            continue;

                        prevFHTargetDay ??= entryRange
                            .Where(x => x.FHTargetDay == "true")
                            .MaxBy(x => x.Datetime);

                        // if we have a prevTargetDay, check for 'alert days' between then and now.  if we already hit tha condition, bail
                        if (prevFHTargetDay != null)
                        {
                            var checkPrevAlerts = entryRange
                                .Where(x => x.Low > lowEnd)
                                .Where(x => x.Low < highEnd)
                                .Any(x => x.Datetime > prevFHTargetDay.Datetime);
                            if (checkPrevAlerts)
                                continue;
                        }

                        if (tempFile.Count == 0)
                            tempFile.Add("Sector,Industry,Category,Symbol");

                        tempFile.Add($"{entry.Sector},{entry.Industry},{entry.Category},{entry.Symbol}");
                    }

                    if (tempFile.Count == 0)
                        break;

                    await File.WriteAllLinesAsync(fName, tempFile, cToken);
                    var messageId = await fhAlert.SendFileAsync(fName, $"FourHandAlerts - Date: [{targetDate:MM/dd/yyyy}]");
                    break;
                }

            case Alerts.IpoAlert:
                {
                    break;
                }
        }
    }

}*/