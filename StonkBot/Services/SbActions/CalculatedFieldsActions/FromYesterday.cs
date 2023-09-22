using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using StonkBot.Data.Entities;
using StonkBot.Extensions;
using StonkBot.Options;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.SbActions._Enums;

namespace StonkBot.Services.SbActions;

internal partial class SbAction
{
    public async Task CalculateFromYesterday(bool recalculateAll, CancellationToken cToken)
    {
        var field = CalcField.FromYesterday;
        var toProcess = recalculateAll
            ? await _db.HistoricalData
                .Include(x => x.CalculatedFields)
                .ToListAsync(cToken)
            : await _db.HistoricalData
                .Include(x => x.CalculatedFields)
                .Where(x => x.CalculatedFields == null || x.CalculatedFields.FromYesterday == null || x.CalculatedFields.FromYesterday == "0.0")
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

                var prevDay = entry.Date.GetPrevTradeDays(1).First();
                var prevDayData = await _db.HistoricalData.FindAsync(new object?[] { entry.Symbol, prevDay }, cToken);
                if (entry is not { Close: > 0 } || prevDayData is not { Close: > 0 })
                {
                    entry.CalculatedFields.FromYesterday = "missingdata";
                    updatedEntries++;
                    continue;
                }
                var calcPercent = ((entry.Close - prevDayData.Close) / prevDayData.Close).ToString("P2");
                entry.CalculatedFields.FromYesterday = calcPercent;
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