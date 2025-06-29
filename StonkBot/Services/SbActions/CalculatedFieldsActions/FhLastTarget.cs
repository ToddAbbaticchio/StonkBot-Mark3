﻿using Microsoft.EntityFrameworkCore;
using StonkBot.Data.Entities;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.SbActions._Enums;
using System.Diagnostics;
using StonkBot.Options;

namespace StonkBot.Services.SbActions;

internal partial class SbAction
{
    public async Task CalculateFhLastTarget(bool recalculateAll, CancellationToken cToken)
    {
        var field = CalcField.LastFhTarget;
        var toProcess = recalculateAll
            ? await _db.HistoricalData
                .Include(x => x.CalculatedFields)
                .ToListAsync(cToken)
            : await _db.HistoricalData
                .Include(x => x.CalculatedFields)
                .Where(x => x.CalculatedFields == null || (x.CalculatedFields.LastFHTarget == null && x.CalculatedFields.FHTargetDay != "true"))
                .ToListAsync(cToken);

        if (toProcess.Count == 0)
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

                var latestRangeDay = await _db.HistoricalData
                    .Where(x => x.Symbol == entry.Symbol)
                    .Where(x => x.Date < entry.Date)
                    .Include(x => x.CalculatedFields)
                    .Where(x => x.CalculatedFields != null && x.CalculatedFields.LastFHTarget != null && x.CalculatedFields.LastFHTarget != "missingdata")
                    .AsSingleQuery()
                    .OrderBy(x => x.Date)
                    .LastOrDefaultAsync(cToken);

                if (latestRangeDay == null || latestRangeDay.CalculatedFields == null)
                {
                    entry.CalculatedFields.LastFHTarget = "missingdata";
                    updatedEntries++;
                    continue;
                }

                entry.CalculatedFields.LastFHTarget = latestRangeDay.CalculatedFields.LastFHTarget;
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