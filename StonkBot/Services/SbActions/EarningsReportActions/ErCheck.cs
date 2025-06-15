using Microsoft.EntityFrameworkCore;
using StonkBot.Data.Entities;
using StonkBot.Extensions;
using StonkBot.MarketPatterns.Models;
using StonkBot.Options;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.DiscordService.Enums;
using StonkBot.Services.DiscordService.Models;

namespace StonkBot.Services.SbActions;

public partial interface ISbAction
{
    Task ErCheck(CancellationToken cToken);
}

internal partial class SbAction
{
    public async Task ErCheck(CancellationToken cToken)
    {
        var allAlertsTable = new DiscordTableMessage(DiscordChannel.EarningsReport, "ALL ER Alerts", "SECTOR,INDUSTRY,CATEGORY,SYMBOL,IS WATCHED,ALERT DATE,ALERT MESSAGE");
        var watchedAlertsTable = new DiscordTableMessage(DiscordChannel.EarningsReport, "Watched ER Alerts Highlight", "SYMBOL,SECTOR,MESSAGE");

        var today = DateTime.Now.SbDate();
        var erList = await _db.EarningsReports
            .Include(x => x.Alerts)
            .GroupBy(x => x.Symbol)
            .Select(g => g
                .OrderByDescending(er => er.Date)
                .First())
            .ToListAsync(cToken);

        // SymbolOverride ////////////////////////////////////
        //erList = erList.Where(x => x.Symbol == "").First(); 
        //////////////////////////////////////////////////////

        var toProcessCount = erList.Count;
        var processed = 0;
        foreach (var er in erList)
        {
            var flaggedAlerts = new List<AlertData>();
            
            try
            {
                // get hData since er.Date
                var hData = await _db.HistoricalData
                    .Where(x => x.Symbol == er.Symbol)
                    .Where(x => x.Date >= er.Date)
                    .Include(x => x.IndustryInfo)
                    .ToListAsync(cToken);
                if (hData.Count == 0)
                    continue;

                // Grab openingDay / dayAfter
                var erDay = hData.SingleOrDefault(x => x.Date == er.Date);
                if (erDay == null)
                    continue;
                var erDayAfter = hData.SingleOrDefault(x => x.Date == er.Date.GetFollowingTradeDay());
                if (erDayAfter == null)
                    continue;

                List<HistoricalData> dataRange;
                switch (er.Time)
                {
                    case "Before Open":
                    {
                        dataRange = hData
                            .Where(x => x.Date >= erDay.Date)
                            .ToList();

                        if (erDay.IsAlwaysUp(dataRange))
                        {
                            flaggedAlerts.AddRange(await _sbPattern.LowDayCheck(er, erDay, dataRange, cToken));
                            flaggedAlerts.AddRange(await _sbPattern.SureLowDayCheck(er, erDay, dataRange, cToken));
                            flaggedAlerts.AddRange(await _sbPattern.StartAlertCheck(er, erDay, dataRange, cToken));
                            flaggedAlerts.AddRange(await _sbPattern.SureStartAlertCheck(er, erDay, dataRange, cToken));
                        }
                        if (erDay.IsAlwaysDown(dataRange))
                        {
                            flaggedAlerts.AddRange(await _sbPattern.HighDayCheck(er, erDay, dataRange, cToken));
                            flaggedAlerts.AddRange(await _sbPattern.SureHighDayCheck(er, erDay, dataRange, cToken));
                            flaggedAlerts.AddRange(await _sbPattern.NegativeStartAlertCheck(er, erDay, dataRange, cToken));
                            flaggedAlerts.AddRange(await _sbPattern.SureNegativeStartAlertCheck(er, erDay, dataRange, cToken));
                        }
                        break;
                    }

                    case "After Close" when erDay.Intersect(erDayAfter) || erDay.ChangeIsSmall(erDayAfter):
                    {
                        var customDay = new HistoricalData
                        {
                            Symbol = erDayAfter.Symbol,
                            Date = erDayAfter.Date,
                            Open = erDayAfter.Open,
                            Close = erDayAfter.Close,
                            Low = new[] { erDay.Low, erDayAfter.Low }.Min(),
                            High = new[] { erDay.High, erDayAfter.High }.Max(),
                            Volume = erDayAfter.Volume,
                            IndustryInfo = erDay.IndustryInfo,
                        };

                        // override previous dataRange
                        dataRange = hData
                            .Where(x => x.Date > customDay.Date)
                            .ToList();

                        if (customDay.IsAlwaysUp(dataRange))
                        {
                            flaggedAlerts.AddRange(await _sbPattern.LowDayCheck(er, customDay, dataRange, cToken));
                            flaggedAlerts.AddRange(await _sbPattern.SureLowDayCheck(er, customDay, dataRange, cToken));
                            flaggedAlerts.AddRange(await _sbPattern.StartAlertCheck(er, customDay, dataRange, cToken));
                            flaggedAlerts.AddRange(await _sbPattern.SureStartAlertCheck(er, customDay, dataRange, cToken));
                        }
                        if (customDay.IsAlwaysDown(dataRange))
                        {
                            flaggedAlerts.AddRange(await _sbPattern.HighDayCheck(er, customDay, dataRange, cToken));
                            flaggedAlerts.AddRange(await _sbPattern.SureHighDayCheck(er, customDay, dataRange, cToken));
                            flaggedAlerts.AddRange(await _sbPattern.NegativeStartAlertCheck(er, customDay, dataRange, cToken));
                            flaggedAlerts.AddRange(await _sbPattern.SureNegativeStartAlertCheck(er, customDay, dataRange, cToken));
                        }

                        break;
                    }

                    case "After Close" when erDay.PositiveChange(erDayAfter):
                    {
                        dataRange = hData
                            .Where(x => x.Date > erDay.Date)
                            .ToList();
                        
                        flaggedAlerts.AddRange(await _sbPattern.JumpHighReverseCheck(er, erDay, erDayAfter, dataRange, cToken));
                        if (!erDay.IsAlwaysUp(dataRange))
                            break;

                        flaggedAlerts.AddRange(await _sbPattern.StartAlertCheck(er, erDay, dataRange, cToken));
                        flaggedAlerts.AddRange(await _sbPattern.SureStartAlertCheck(er, erDay, dataRange, cToken));
                        flaggedAlerts.AddRange(await _sbPattern.HighHalfCheck(er, erDay, erDayAfter, dataRange, cToken));
                        flaggedAlerts.AddRange(await _sbPattern.HighThirdCheck(er, erDay, erDayAfter, dataRange, cToken));
                        flaggedAlerts.AddRange(await _sbPattern.JumpHighCheck(er, erDay, erDayAfter, dataRange, cToken));
                        break;
                    }

                    case "After Close" when !erDay.PositiveChange(erDayAfter):
                    {
                        dataRange = hData
                            .Where(x => x.Date > erDay.Date)
                            .ToList();

                        flaggedAlerts.AddRange(await _sbPattern.JumpLowReverseCheck(er, erDay, erDayAfter, dataRange, cToken));
                        if (!erDay.IsAlwaysDown(dataRange))
                            break;

                        flaggedAlerts.AddRange(await _sbPattern.NegativeStartAlertCheck(er, erDay, dataRange, cToken));
                        flaggedAlerts.AddRange(await _sbPattern.SureNegativeStartAlertCheck(er, erDay, dataRange, cToken));
                        flaggedAlerts.AddRange(await _sbPattern.LowHalfCheck(er, erDay, erDayAfter, dataRange, cToken));
                        flaggedAlerts.AddRange(await _sbPattern.LowThirdCheck(er, erDay, erDayAfter, dataRange, cToken));
                        flaggedAlerts.AddRange(await _sbPattern.JumpLowCheck(er, erDay, erDayAfter, dataRange, cToken));
                        break;
                    }
                    
                    case "":
                    case null:
                    {
                        var customDay = new HistoricalData
                        {
                            Symbol = erDayAfter.Symbol,
                            Date = erDayAfter.Date,
                            Open = erDayAfter.Open,
                            Close = erDayAfter.Close,
                            Low = new[] { erDay.Low, erDayAfter.Low }.Min(),
                            High = new[] { erDay.High, erDayAfter.High }.Max(),
                            Volume = erDayAfter.Volume,
                            IndustryInfo = erDay.IndustryInfo,
                        };

                        // override previous dataRange
                        dataRange = hData
                            .Where(x => x.Date > customDay.Date)
                            .ToList();

                        if (erDay.IsAlwaysUp(dataRange))
                        {
                            flaggedAlerts.AddRange(await _sbPattern.LowDayCheck(er, customDay, dataRange, cToken));
                            flaggedAlerts.AddRange(await _sbPattern.SureLowDayCheck(er, customDay, dataRange, cToken));
                            flaggedAlerts.AddRange(await _sbPattern.StartAlertCheck(er, customDay, dataRange, cToken));
                            flaggedAlerts.AddRange(await _sbPattern.SureStartAlertCheck(er, customDay, dataRange, cToken));
                        }
                        if (erDay.IsAlwaysDown(dataRange))
                        {
                            flaggedAlerts.AddRange(await _sbPattern.HighDayCheck(er, customDay, dataRange, cToken));
                            flaggedAlerts.AddRange(await _sbPattern.SureHighDayCheck(er, customDay, dataRange, cToken));
                            flaggedAlerts.AddRange(await _sbPattern.NegativeStartAlertCheck(er, customDay, dataRange, cToken));
                            flaggedAlerts.AddRange(await _sbPattern.SureNegativeStartAlertCheck(er, customDay, dataRange, cToken));
                        }
                        break;
                    }
                }

                foreach (var alert in flaggedAlerts)
                {
                    // Ignore alerts that already exist for today
                    if (er.Alerts.Any(x => x.Type == alert.AlertType.ToString() && x.Date == alert.Date))
                        continue;
                    
                    // Add watched alerts to watched table
                    if (alert.IsWatched == "WATCHED")
                        watchedAlertsTable.Data.Add(alert.ToTableMessage("Watched"));

                    // Add to AllAlerts table
                    allAlertsTable.Data.Add(alert.ToTableMessage("All"));

                    // Add er.Alert
                    er.Alerts.Add(alert.GenerateErAlert());
                }
            }
            catch (Exception ex)
            {
                _con.WriteLog(MessageSeverity.Error, $"Error processing ER: {er.Symbol}: {(ex.InnerException != null ? ex.InnerException.Message : ex.Message)}");
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

        if (watchedAlertsTable.ContainsData())
            await _discordClient.PostTableAsync(watchedAlertsTable, today, cToken);

        if (allAlertsTable.ContainsData())
            await _discordClient.PostFileAsync(allAlertsTable, cToken);

        await _db.SbSaveChangesAsync(cToken);
    }
}