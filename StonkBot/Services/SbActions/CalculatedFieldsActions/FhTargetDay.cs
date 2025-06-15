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
    public async Task CalculateFhTargetDay(bool recalculateAll, CancellationToken cToken)
    {
        var field = CalcField.FhTargetDay;
        var toProcess = recalculateAll
            ? await _db.HistoricalData
                .Include(x => x.CalculatedFields)
                .ToListAsync(cToken)
            : await _db.HistoricalData
                .Include(x => x.CalculatedFields)
                .Where(x => x.CalculatedFields == null || x.CalculatedFields.FHTargetDay == null)
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

                var priorDates = entry.Date.GetPrevTradeDays(3);
                var priorDatesData = await _db.HistoricalData
                    .Where(x => x.Symbol == entry.Symbol)
                    .Where(x => priorDates.Contains(x.Date))
                    .Include(x => x.CalculatedFields)
                    .AsSingleQuery()
                    .ToListAsync(cToken);

                if (priorDatesData.Count != 3)
                {
                    entry.CalculatedFields.FHTargetDay = "missingdata";
                    updatedEntries++;
                    continue;
                }

                // test on 4 days (today + prev 3)
                priorDatesData.Add(entry);
                var s = priorDatesData
                    .OrderByDescending(x => x.Date)
                    .ToArray();

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
                    entry.CalculatedFields.FHTargetDay = "false";
                    updatedEntries++;
                    continue;
                }

                entry.CalculatedFields.FHTargetDay = "true";
                // dan's new rule
                if (s[3].Close * (decimal)1.01 <= s[2].Open)
                {
                    entry.CalculatedFields.LastFHTarget = $"{s[3].Low},{s[3].High}";
                    updatedEntries++;
                    continue;
                }
                // the normal rule
                entry.CalculatedFields.LastFHTarget = $"{s[2].Low},{s[2].High}";
                updatedEntries++;
                continue;
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