using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using StonkBot.Data.Entities;
using StonkBot.Extensions;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.SbActions._Enums;

namespace StonkBot.Services.SbActions;

internal partial class SbAction
{
    public async Task CalculateFiveDayStable(bool recalculateAll, CancellationToken cToken)
    {
        var field = CalcField.FiveDayStable;
        var toProcess = recalculateAll
            ? await _db.HistoricalData
                .Include(x => x.CalculatedFields)
                .ToListAsync(cToken)
            : await _db.HistoricalData
                .Include(x => x.CalculatedFields)
                .Where(x => x.CalculatedFields == null || string.IsNullOrEmpty(x.CalculatedFields.FiveDayStable))
                .ToListAsync(cToken);

        if (!toProcess.Any())
            return;

        _con.WriteLog(MessageSeverity.Info, TargetLog.ActionRunner, $"Processing DB entries missing '{field}' field...");
        var toProcessCount = toProcess.Count;
        var updatedEntries = 0;
        var processed = 0;
        var timer = new Stopwatch();
        timer.Start();

        foreach (var entry in toProcess)
        {
            try
            {
                // if calcfields is null create and add one
                entry.CalculatedFields ??= new CalculatedFields
                {
                    Symbol = entry.Symbol,
                    Date = entry.Date,
                };

                var prevDates = entry.Date.GetPrevTradeDays(5);
                var prevDatesDataList = await _db.HistoricalData
                    .Where(x => x.Symbol == entry.Symbol)
                    .Where(x => prevDates.Contains(x.Date))
                    .Include(x => x.CalculatedFields)
                    .AsSingleQuery()
                    .ToListAsync(cToken);

                if (prevDatesDataList.Count < 5)
                {
                    entry.CalculatedFields.FiveDayStable = "missingdata";
                    updatedEntries++;
                    continue;
                }

                var combinedList = new List<decimal>();
                combinedList.AddRange(prevDatesDataList.Select(x => x.Open).ToList());
                combinedList.AddRange(prevDatesDataList.Select(x => x.Close).ToList());
                var minVal = combinedList.Min(x => x);
                var maxVal = combinedList.Max(x => x);

                if (maxVal <= minVal * (decimal)1.1 && maxVal > minVal * (decimal)1.05)
                {
                    entry.CalculatedFields.FiveDayStable = "StrictMatch";
                    updatedEntries++;
                    continue;
                }

                if (maxVal <= minVal * (decimal)1.1)
                {
                    entry.CalculatedFields.FiveDayStable = "GenerousMatch";
                    updatedEntries++;
                    continue;
                }

                entry.CalculatedFields.FiveDayStable = "nomatch";
                updatedEntries++;
            }
            catch (Exception ex)
            {
                _con.WriteLog(MessageSeverity.Error, $"Error processing {entry.Symbol}-{entry.Date} for missing '{field}' field: {ex.Message}");
            }
            finally
            {
                processed++;
                if (processed % Constants.ProgressTick == 0)
                    _con.WriteProgress(processed, toProcessCount);
                if (processed == toProcessCount)
                    _con.WriteProgressComplete("ProcessingComplete!");
            }
        }

        if (updatedEntries > 0)
        {
            _con.WriteLog(MessageSeverity.Info, $"Processed {toProcessCount} entries missing '{field}' field. Committing changes to DB...");
            await _db.SbSaveChangesAsync(cToken);
            timer.Stop();
            _con.WriteLog(MessageSeverity.Info, $"Done! Elapsed time: [{timer.Elapsed}] Averaging [{Convert.ToSingle(timer.ElapsedMilliseconds) / Convert.ToSingle(updatedEntries)}]ms per entry updated!");
            return;
        }

        timer.Stop();
        _con.WriteLog(MessageSeverity.Info, $"Found {toProcessCount} entries missing '{field}' field in [{timer.Elapsed}] but none were fixable.");
    }
}