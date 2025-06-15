using StonkBot.Data.Entities;
using StonkBot.Services.ConsoleWriter.Enums;
using System.Diagnostics;

namespace StonkBot.Services.SbActions;

public partial interface ISbAction
{
    Task ScrapeMostActive(CancellationToken cToken);
}

internal partial class SbAction
{
    public async Task ScrapeMostActive(CancellationToken cToken)
    {
        _con.WriteLog(MessageSeverity.Section, _targetLog, "SbActionRunner.IndustryInfoScrape - Starting...");
        var timer = new Stopwatch();
        timer.Start();

        try
        {
            var mostActive = _db.MostActiveRecords;
            var scrapedList = await _webScraper.ScrapeMostActiveData(cToken);
            if (scrapedList == null || scrapedList.Count == 0)
            {
                _con.WriteLog(MessageSeverity.Warning, "MostActive scrape returned nothing. Is a Problem?");
                return;
            }

            var dbUpdates = 0;
            var toAdd = new List<MostActive>();
            foreach (var record in scrapedList)
            {
                var dbMatch = await mostActive.FindAsync(new object?[] { record!.Symbol, record.Date }, cToken);
                if (dbMatch != null)
                    continue;

                // create new obj if not exist
                toAdd.Add(record);
                dbUpdates++;
            }

            if (dbUpdates > 0)
            {
                _con.WriteLog(MessageSeverity.Info, TargetLog.ActionRunner, $"Found {dbUpdates} new 'MostActive' Symbol Records! Adding to database...");
                await mostActive.AddRangeAsync(toAdd, cToken);
                await _db.SbSaveChangesAsync(cToken);
            }
        }
        catch (Exception ex)
        {
            _con.WriteLog(MessageSeverity.Error, $"Error during StonkBotActions.ScrapeMostActive: {ex.Message}");
        }

        timer.Stop();
        _con.WriteLog(MessageSeverity.Stats, _targetLog, $"Elapsed time: [{timer.Elapsed}]");
    }
}