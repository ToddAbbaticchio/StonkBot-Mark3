using StonkBot.Data.Entities;
using StonkBot.Services.ConsoleWriter.Enums;
using System.Diagnostics;
using System.Reflection;

namespace StonkBot.Services.SbActions;

public partial interface ISbAction
{
    Task IndustryInfoScrape(CancellationToken cToken);
}

internal partial class SbAction
{
    public async Task IndustryInfoScrape(CancellationToken cToken)
    {
        _con.WriteLog(MessageSeverity.Section, _targetLog, "SbActionRunner.IndustryInfoScrape - Starting...");
        var timer = new Stopwatch();
        timer.Start();

        try
        {
            var scrapedList = await _webScraper.ScrapeIndustryInfoData(cToken);
            if (scrapedList.Count == 0)
            {
                _con.WriteLog(MessageSeverity.Warning, "IndustryInfoScrape returned nothing. Is a Problem?");
                return;
            }

            var industryInfos = _db.IndustryInfo.ToList();
            var dbUpdates = 0;
            var newItems = 0;
            var hDataList = new List<IndustryInfoHData>();
            foreach (var pair in scrapedList)
            {
                var iInfo = pair.Item1;
                hDataList.Add(pair.Item2);

                var dbItem = industryInfos.FirstOrDefault(x => x.Symbol == iInfo.Symbol);
                if (dbItem == null)
                {
                    _db.IndustryInfo.Add(iInfo);
                    newItems++;
                    continue;
                }
                if (dbItem.Category != iInfo.Category)
                {
                    dbItem.Category = iInfo.Category;
                    dbUpdates++;
                }
            }

            var hDataCount = hDataList.Count;
            if (hDataCount > 0)
            {
                _con.WriteLog(MessageSeverity.Info, TargetLog.ActionRunner, $"Acquired daily data for {hDataCount} Industries!  Adding to database...");
                _db.IndustryInfoHData.AddRange(hDataList);
            }

            if (dbUpdates > 0 || newItems > 0)
            {
                _con.WriteLog(MessageSeverity.Info, TargetLog.ActionRunner, $"Found {newItems} new Industry Symbols and updated {dbUpdates} with Categories that changed! Adding to database...");
                await _db.SbSaveChangesAsync(cToken);
            }
        }
        catch (Exception ex)
        {
            _con.WriteLog(MessageSeverity.Error, $"Error during StonkBotActions.IndustryInfoScrape: {ex.Message}");
        }

        timer.Stop();
        _con.WriteLog(MessageSeverity.Stats, _targetLog, $"Elapsed time: [{timer.Elapsed}]");
    }
}