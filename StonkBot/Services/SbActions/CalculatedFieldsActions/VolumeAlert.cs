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
    public async Task CalculateVolumeAlert(bool recalculateAll, CancellationToken cToken)
    {
        var field = CalcField.VolumeAlert;
        var toProcess = recalculateAll
            ? await _db.HistoricalData
                .Include(x => x.CalculatedFields)
                .ToListAsync(cToken)
            : await _db.HistoricalData
                .Include(x => x.CalculatedFields)
                .Where(x => x.CalculatedFields == null || string.IsNullOrEmpty(x.CalculatedFields.VolumeAlert))
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

                var prevDates = entry.Date.GetPrevTradeDays(5);
                var prevDatesDataList = await _db.HistoricalData
                    .Where(x => x.Symbol == entry.Symbol)
                    .Where(x => prevDates.Contains(x.Date))
                    .Include(x => x.CalculatedFields)
                    .AsSingleQuery()
                    .ToListAsync(cToken);

                if (prevDatesDataList.Count < 5)
                {
                    entry.CalculatedFields.VolumeAlert = "missingdata";
                    updatedEntries++;
                    continue;
                }

                var totalVolume = prevDatesDataList.Sum(x => x.Volume);
                if (totalVolume == 0)
                {
                    entry.CalculatedFields.VolumeAlert = "missingdata";
                    updatedEntries++;
                    continue;
                }

                var avgVolume = totalVolume / prevDatesDataList.Count;
                var volPercent = entry.Volume / avgVolume;

                if (volPercent is >= (decimal)1.2 and <= (decimal)1.6)
                {
                    entry.CalculatedFields.VolumeAlert = volPercent.ToString("P2");
                    updatedEntries++;
                    continue;
                }

                entry.CalculatedFields.VolumeAlert = "false";
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