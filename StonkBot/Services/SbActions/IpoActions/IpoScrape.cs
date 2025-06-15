using System.Diagnostics;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.ConsoleWriter.Models;

namespace StonkBot.Services.SbActions;

public partial interface ISbAction
{
    Task IpoScrape(CancellationToken cToken);
}

internal partial class SbAction
{
    public async Task IpoScrape(CancellationToken cToken)
    {
        _con.WriteLog(MessageSeverity.Section, _targetLog, "SbActionRunner.IpoScrape - Starting...");
        var timer = new Stopwatch();
        timer.Start();

        try
        {
            var dbIpos = _db.IpoListings;

            var scrapedList = await _webScraper.ScrapeIpoData(cToken);
            if (scrapedList.Count == 0)
            {
                _con.WriteLog(MessageSeverity.Warning, "IpoScrape returned nothing. Is a Problem?");
                return;
            }

            var dbChanged = false;
            List<LogEntry> logList = new();
            foreach (var ipo in scrapedList)
            {
                var dbMatch = await dbIpos.FindAsync(new object?[] { ipo.Symbol }, cToken);
                
                // create new ipo if not exist
                if (dbMatch == null)
                {
                    await _db.IpoListings.AddAsync(ipo, cToken);
                    dbChanged = true;
                    logList.Add(new LogEntry(MessageSeverity.Info, TargetLog.ActionRunner, $"Created new entry for for {ipo.Symbol}"));
                    continue;
                }

                // If everything matches do nothing
                if (ipo.ExpectedListingDate == dbMatch.ExpectedListingDate &&
                    ipo.OfferingEndDate == dbMatch.OfferingEndDate &&
                    ipo.OfferingPrice == dbMatch.OfferingPrice &&
                    ipo.OfferAmmount == dbMatch.OfferAmmount)
                    continue;

                // Update items with unmatched data
                dbMatch.ExpectedListingDate = ipo.ExpectedListingDate;
                dbMatch.OfferingEndDate = ipo.OfferingEndDate;
                dbMatch.OfferingPrice = ipo.OfferingPrice;
                dbMatch.OfferAmmount = ipo.OfferAmmount;
                dbChanged = true;
                logList.Add(new LogEntry(MessageSeverity.Info, TargetLog.ActionRunner, $"Updated existing entry for {ipo.Symbol}"));
            }

            if (dbChanged)
                await _db.SbSaveChangesAsync(cToken);

            if (logList.Count != 0)
                logList.ForEach(logEntry => _con.WriteLog(logEntry));
        }
        catch (Exception ex)
        {
            _con.WriteLog(MessageSeverity.Error, $"Error during StonkBotActions.IpoScrape: {ex.Message}");
        }

        timer.Stop();
        _con.WriteLog(MessageSeverity.Info, _targetLog, $"Elapsed time: [{timer.Elapsed}]");
    }
}