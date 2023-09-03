using StonkBot.Services.ConsoleWriter.Enums;

namespace StonkBot.Services.SbActions;

internal partial class SbAction
{
    public async Task ErScrape(CancellationToken cToken)
    {
        try
        {
            //await using var _db = new StonkBotDbContext();

            var dbEarningsReports = _db.EarningsReports;

            var scrapedList = await _webScraper.ScrapeErData(cToken);
            if (scrapedList == null || !scrapedList.Any())
            {
                _con.WriteLog(MessageSeverity.Warning, "ErScrape returned nothing. Is a Problem?");
                return;
            }

            var dbUpdates = 0;
            foreach (var er in scrapedList)
            {
                var dbMatch = await dbEarningsReports.FindAsync(new object?[] { er!.Symbol, er.Date }, cToken);
                if (dbMatch != null)
                    continue;

                // create new obj if not exist
                await _db.EarningsReports.AddAsync(er, cToken);
                dbUpdates++;
            }

            if (dbUpdates > 0)
            {
                _con.WriteLog(MessageSeverity.Info, TargetLog.ActionRunner, $"Found {dbUpdates} new EarningsReports! Adding to database...");
                await _db.SbSaveChangesAsync(cToken);
            }
        }
        catch (Exception ex)
        {
            _con.WriteLog(MessageSeverity.Error, $"Error during StonkBotActions.IpoScrape: {ex.Message}");
        }
    }
}