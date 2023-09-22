using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using StonkBot.Data.Entities;
using StonkBot.Options;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.SbActions._Enums;

namespace StonkBot.Services.SbActions;

internal partial class SbAction
{
    public async Task CalculateAboveUpperShadow(bool recalculateAll, CancellationToken cToken)
    {
        var field = CalcField.AboveUpperShadow;
        var toProcess = recalculateAll
            ? await _db.HistoricalData
                .Include(x => x.CalculatedFields)
                .ToListAsync(cToken)
            : await _db.HistoricalData
                .Include(x => x.CalculatedFields)
                .Where(x => x.CalculatedFields == null || x.CalculatedFields.AboveUpperShadow == null)
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

                var priorData = await _db.HistoricalData
                    .Where(x => x.Symbol == entry.Symbol)
                    .Where(x => x.Date < entry.Date)
                    .Include(x => x.CalculatedFields)
                    .AsSingleQuery()
                    .ToListAsync(cToken);

                var lastUpperShadowDay = priorData
                    .Where(x => x.CalculatedFields?.UpperShadow == true)
                    .MaxBy(x => x.Date);
                
                if (lastUpperShadowDay == null)
                {
                    entry.CalculatedFields.AboveUpperShadow = "false";
                    updatedEntries++;
                    continue;
                }

                if (entry.Close > lastUpperShadowDay.High)
                {
                    var alreadyNotified = priorData
                        .Where(x => x.CalculatedFields?.AboveUpperShadow == "firsttrue")
                        .FirstOrDefault(x => x.Date > lastUpperShadowDay.Date);

                    if (alreadyNotified == null)
                    {
                        entry.CalculatedFields.AboveUpperShadow = "firsttrue";
                        updatedEntries++;
                        continue;
                    }

                    entry.CalculatedFields.AboveUpperShadow = "true";
                    updatedEntries++;
                    continue;
                }

                entry.CalculatedFields.AboveUpperShadow = "false";
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