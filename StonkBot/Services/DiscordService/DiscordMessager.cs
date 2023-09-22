using Discord;
using Discord.Webhook;
using Microsoft.EntityFrameworkCore;
using StonkBot.Data;
using StonkBot.Data.Entities;
using StonkBot.Data.Enums;
using StonkBot.Extensions;
using StonkBot.Options;
using StonkBot.Services.ConsoleWriter;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.DiscordService.Enums;
using StonkBot.Services.SbActions._Models;

namespace StonkBot.Services.DiscordService;

public interface IDiscordMessager
{
    Task SendAlertAsync(DiscordChannel channel, string message, CancellationToken cToken);
    Task SendFileAsync(DiscordChannel channel, List<string> fileText, DateTime targetDate, CancellationToken cToken);
    Task<List<ulong>> PostTableAsync(DiscordChannel channel, string tableHeading, List<List<string>> bodyData, DateTime targetDate, CancellationToken cToken);


    Task SendIpoFirstPassAlertsAsync(List<IpoFirstPassAlert> alertList, CancellationToken cToken);
    Task SendIpoSecondPassAlertsAsync(List<IpoSecondPassAlert> alertList, CancellationToken cToken);
}

public class DiscordMessager : IDiscordMessager
{
    private readonly IConsoleWriter _con;
    private readonly StonkBotDbContext _db;
    private readonly TargetLog _logWindow;
    private readonly SbVars _vars;
    
    public DiscordMessager(IConsoleWriter con, StonkBotDbContext db, SbVars vars)
    {
        _con = con;
        _db = db;
        _logWindow = TargetLog.ActionRunner;
        _vars = vars;
    }

    private string GetWebhookUrl(DiscordChannel channel)
    {
        var url = channel switch
        {
            DiscordChannel.IpoWatch => _vars.IpoWebhook,
            DiscordChannel.VolAlert => _vars.VolumeAlertWebhook,
            DiscordChannel.VolAlert2 => _vars.VolumeAlertWebhook,
            DiscordChannel.UpperShadow => _vars.UpperShadowWebhook,
            DiscordChannel.FourHand => _vars.FourHandWebhook,
            DiscordChannel.EarningsReport => _vars.EarningsReportWebhook,
            _ => ""
        };

        if (string.IsNullOrEmpty(url))
            _con.WriteLog(MessageSeverity.Error, TargetLog.ActionRunner, $"Resolved an empty/null webhook url for channel: {channel}");

        return url;
    }

    public async Task SendAlertAsync(DiscordChannel channel, string message, CancellationToken cToken)
    {
        var dbMessageRecords = _db.DiscordMessageRecords;
        var webhookUrl = GetWebhookUrl(channel);

        var client = new DiscordWebhookClient(webhookUrl);
        var messageId = await client.SendMessageAsync(message);
        
        await dbMessageRecords.AddAsync(new DiscordMessageRecord
        {
            MessageId = messageId,
            DateTime = DateTime.Today.SbDate(),
            Channel = channel.ToString(),
        }, cToken);

        await _db.SbSaveChangesAsync(cToken);
    }

    public async Task SendFileAsync(DiscordChannel channel, List<string> fileText, DateTime targetDate, CancellationToken cToken)
    {
        var fName = $"C:/temp/{channel}.csv";
        await File.WriteAllLinesAsync(fName, fileText, cToken);

        var dbMessageRecords = _db.DiscordMessageRecords;
        var webhookUrl = GetWebhookUrl(channel);

        var client = new DiscordWebhookClient(webhookUrl);
        var messageId = await client.SendFileAsync(fName, $"{channel} - Date: [{targetDate.SbDateString()}]");
        await dbMessageRecords.AddAsync(new DiscordMessageRecord
        {
            MessageId = messageId,
            DateTime = DateTime.Now.SbDate(),
            Channel = channel.ToString()
        }, cToken);
        await _db.SbSaveChangesAsync(cToken);
    }

    public async Task<List<ulong>> PostTableAsync(DiscordChannel channel, string tableHeading, List<List<string>> bodyData, DateTime targetDate, CancellationToken cToken)
    {
        var sentMessages = new List<ulong>();
        var webhookUrl = GetWebhookUrl(channel);
        var client = new DiscordWebhookClient(webhookUrl);
        ulong messageId;

        // Get max width for each column-to-be
        var columnCount = bodyData[0].Count;
        var maxWidths = new List<int>();
        maxWidths.AddRange(Enumerable.Repeat(0, columnCount));
        foreach (var row in bodyData)
        {
            for (var i = 0; i < row.Count; i++)
            {
                var cellLength = row[i].Length;
                if (cellLength > maxWidths[i])
                    maxWidths[i] = cellLength;
            }
        }
        
        // Create raw text table
        var msgBody = $"```\r\n{tableHeading} - Date: [{targetDate.SbDateString()}]\r\n";
        foreach (var row in bodyData)
        {
            for (var i = 0; i < columnCount; i++)
            {
                if (i == 0)
                {
                    msgBody += $"{row[i].PadLeft(maxWidths[i])}";
                }
                else if (i == columnCount - 1)
                {
                    msgBody += $"  |  {row[i].PadLeft(maxWidths[i])}\r\n";
                }
                else
                {
                    msgBody += $"  |  {row[i].PadLeft(maxWidths[i])}";
                }
            }

            if (msgBody.Length > 1800)
            {
                msgBody += "```";
                messageId = await client.SendMessageAsync(msgBody);
                sentMessages.Add(messageId);

                msgBody = "```\r\n";
            }
        }
        msgBody += "```";
        messageId = await client.SendMessageAsync(msgBody);
        sentMessages.Add(messageId);

        return sentMessages;
    }

    public async Task SendIpoFirstPassAlertsAsync(List<IpoFirstPassAlert> alertList, CancellationToken cToken)
    {
        var alertDay = (alertList.FirstOrDefault()!).TodayDate;
        var client = new DiscordWebhookClient(_vars.IpoWebhook);

        var alreadyPosted = _db.DiscordMessageRecords
            .Where(x => x.DateTime == alertDay)
            .Any(x => x.Type == AlertType.IpoFirstPassTable);
        if (alreadyPosted)
            return;
        
        var alertedSymbols = alertList
            .Select(x => x.Symbol)
            .ToList();

        var bodyData = new List<List<string>> { new() { "SYMBOL", "DAYS SATISFIED", "TODAY CLOSE", "OPENING DAY HIGH" } };
        alertList.ForEach(x => bodyData.Add(new List<string> { x.Symbol, x.DaysSatisfied.ToString(), x.TodayClose, x.OpeningHigh }));
        var messages = await PostTableAsync(DiscordChannel.IpoWatch, "IPO Alerts 1stPass", bodyData, alertDay, cToken);
        if (!messages.Any())
            return;

        var messageRecord = new DiscordMessageRecord
        {
            MessageId = messages.First(),
            Channel = DiscordChannel.IpoWatch.ToString(),
            DateTime = alertDay,
            Type = AlertType.IpoFirstPassTable
        };
        await _db.DiscordMessageRecords.AddAsync(messageRecord, cToken);
        
        await _db.IpoListings
            .Where(x => alertedSymbols.Contains(x.Symbol))
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.FirstPassDate, p => alertDay), cToken);
        await _db.SbSaveChangesAsync(cToken);
        alertedSymbols.ForEach(x => _con.WriteLog(MessageSeverity.Info, _logWindow, $"Added 'FirstPass' discordMessageRecords for watched Ipo:{x}"));
    }

    public async Task SendIpoSecondPassAlertsAsync(List<IpoSecondPassAlert> alertList, CancellationToken cToken)
    {
        var dbIpos = _db.IpoListings;
        var alertDay = (alertList.FirstOrDefault()!).TargetDate;
        var client = new DiscordWebhookClient(_vars.IpoWebhook);
        var alertedSymbols = alertList
            .Select(x => x.Symbol)
            .ToList();

        var f1MaxLen = alertList.Select(x => x.FirstCheckDate.SbDateString()).MaxBy(x => x.Length)!.Length;
        var f2MaxLen = alertList.Select(x => x.CheckDayClose).MaxBy(x => x.Length)!.Length;
        var f3MaxLen = alertList.Select(x => x.OpeningDayLow).MaxBy(x => x.Length)!.Length;
        var f4MaxLen = alertList.Select(x => x.TodayClose).MaxBy(x => x.Length)!.Length;
        var f5MaxLen = alertList.Select(x => x.OpeningDayOpen).MaxBy(x => x.Length)!.Length;

        var msgBody = $"```\r\nIPO Alerts 2ndPass - Date: [{alertDay.SbDateString()}]\r\n";
        alertList.ForEach(x => msgBody += 
            $"{x.Symbol,5}" +
            $"  |  Days Satisfied: [{x.DaysSatisfied,3}]" +
            $"  |  {x.FirstCheckDate.SbDateString().PadLeft(f1MaxLen)}Close:[{x.CheckDayClose.PadLeft(f2MaxLen)}]" +
            $"  |  IpoDayLow:[{x.OpeningDayLow.PadLeft(f3MaxLen)}]" +
            $"  |  TodayClose:[{x.TodayClose.PadLeft(f4MaxLen)}]" +
            $"  |  OpeningDayOpen:[{x.OpeningDayOpen.PadLeft(f5MaxLen)}]\r\n");
        msgBody += "```";

        await client.SendMessageAsync(msgBody);
        await dbIpos
            .Where(x => alertedSymbols.Contains(x.Symbol))
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.LastSecondPassDate, p => alertDay), cToken);
        await _db.SbSaveChangesAsync(cToken);
        alertedSymbols.ForEach(x => _con.WriteLog(MessageSeverity.Info, _logWindow, $"Sent 'SecondPass' DiscordAlert for watched Ipo:{x}"));
    }
}