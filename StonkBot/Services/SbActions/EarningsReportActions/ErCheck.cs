using System.Diagnostics;
using System.Threading.Channels;
using Microsoft.EntityFrameworkCore;
using StonkBot.Data.Entities;
using StonkBot.Data.Enums;
using StonkBot.Extensions;
using StonkBot.MarketPatterns.Models;
using StonkBot.Options;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.DiscordService.Enums;

namespace StonkBot.Services.SbActions;

public partial interface ISbAction
{
    Task ErCheck(CancellationToken cToken);
}

internal partial class SbAction
{
    public async Task ErCheck(CancellationToken cToken)
    {
        const string headerRow = "Sector,Industry,Category,Symbol,IsWatched,AlertDate,AlertMessage";
        var discordMessageStrings = new List<string> { headerRow };
        var today = DateTime.Now.SbDate();
        var erSymbols = await _db.EarningsReports
            .Select(x => x.Symbol)
            .Distinct()
            .OrderBy(x => x)
            .ToListAsync(cToken);

        // SymbolOverride ////////////////////////////////////
        //erSymbols = new List<string> { "CVNA" };
        //////////////////////////////////////////////////////

        var toProcessCount = erSymbols.Count;
        var processed = 0;
        foreach (var symbol in erSymbols)
        {
            var flaggedAlerts = new List<AlertData>();
            
            try
            {
                // get latest earnings report
                var er = _db.EarningsReports
                    .Where(x => x.Symbol == symbol)
                    .Include(x => x.Alerts)
                    .AsSingleQuery()
                    .ToList()
                    .MaxBy(x => x.Date);

                if (er == null)
                {
                    _con.WriteLog(MessageSeverity.Warning, _targetLog, $"Unable to retrieve earnings report for {symbol}");
                    continue;
                }

                // get hData since er.Date
                var hData = await _db.HistoricalData
                    .Where(x => x.Symbol == symbol)
                    .Where(x => x.Date >= er.Date)
                    .Include(x => x.IndustryInfo)
                    .ToListAsync(cToken);
                if (!hData.Any())
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

                // Watched alert table
                var watchedAlerts = flaggedAlerts.Where(x => x.IsWatched == "WATCHED").ToList();
                var alreadyPosted = _db.DiscordMessageRecords
                    .Where(x => x.DateTime == today)
                    .Any(x => x.Type == AlertType.WatchedAlertTable);
                
                if (watchedAlerts.Any() && !alreadyPosted)
                {
                    var bodyData = new List<List<string>> { new() { "SYMBOL", "SECTOR", "MESSAGE" } };
                    watchedAlerts.ForEach(x => bodyData.Add(new List<string>{x.Symbol, x.Sector!, x.Message}));

                    var messages = await _discordClient.PostTableAsync(DiscordChannel.EarningsReport, "Watched Earnings Reports Alerts", bodyData, today, cToken);
                    if (messages.Any())
                    {
                        var messageRecord = new DiscordMessageRecord
                        {
                            MessageId = messages.First(),
                            Channel = DiscordChannel.EarningsReport.ToString(),
                            DateTime = today,
                            Type = AlertType.WatchedAlertTable
                        };

                        await _db.DiscordMessageRecords.AddAsync(messageRecord, cToken);
                    }
                }
                
                // Big message
                foreach (var alert in flaggedAlerts)
                {
                    er.Alerts.Add(alert.GenerateErAlert());
                    discordMessageStrings.Add(alert.GetMessageString());
                }
            }
            catch (Exception ex)
            {
                _con.WriteLog(MessageSeverity.Error, $"Error processing ER: {symbol}: {ex.Message}");
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

        if (discordMessageStrings.Any(x => x != headerRow))
        {
            await _discordClient.SendFileAsync(DiscordChannel.EarningsReport, discordMessageStrings, today, cToken);
            await _db.SbSaveChangesAsync(cToken);
        }
    }
}