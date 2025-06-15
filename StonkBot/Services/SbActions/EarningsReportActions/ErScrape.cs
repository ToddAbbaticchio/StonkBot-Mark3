using StonkBot.Data.Entities;
using StonkBot.Services.ConsoleWriter.Enums;

namespace StonkBot.Services.SbActions;

internal partial class SbAction
{
    public async Task ErScrape(CancellationToken cToken)
    {
        try
        {
            var dbEarningsReports = _db.EarningsReports;
            var scrapedList = await _webScraper.ScrapeErData(cToken);
            if (scrapedList == null || scrapedList.Count == 0)
            {
                _con.WriteLog(MessageSeverity.Warning, "ErScrape returned nothing. Is a Problem?");
                return;
            }

            var dbUpdates = 0;
            var toAdd = new List<EarningsReport>();
            foreach (var er in scrapedList)
            {
                var dbMatch = await dbEarningsReports.FindAsync(new object?[] { er!.Symbol, er.Date }, cToken);
                if (dbMatch != null)
                    continue;

                toAdd.Add(er);
                dbUpdates++;
            }

            if (dbUpdates > 0)
            {
                _con.WriteLog(MessageSeverity.Info, TargetLog.ActionRunner, $"Found {dbUpdates} new EarningsReports! Adding to database...");
                await dbEarningsReports.AddRangeAsync(toAdd, cToken);
                await _db.SbSaveChangesAsync(cToken);
            }
        }
        catch (Exception ex)
        {
            _con.WriteLog(MessageSeverity.Error, $"Error during StonkBotActions.ErScrape: {ex.Message}");
        }
    }
}