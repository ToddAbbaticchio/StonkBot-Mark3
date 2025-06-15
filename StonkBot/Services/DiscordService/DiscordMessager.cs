using Discord.Webhook;
using Microsoft.EntityFrameworkCore;
using StonkBot.Data;
using StonkBot.Data.Entities;
using StonkBot.Data.Enums;
using StonkBot.Extensions;
using StonkBot.Services.ConsoleWriter;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.DiscordService.Enums;
using StonkBot.Services.DiscordService.Models;
using StonkBot.Services.SbActions._Models;
using System.Text;
using Microsoft.Extensions.Options;
using StonkBot.Appsettings.Models;

namespace StonkBot.Services.DiscordService;

public interface IDiscordMessager
{
    Task SendAlertAsync(DiscordChannel channel, string message, CancellationToken cToken);

    Task SendFileAsync(DiscordChannel channel, List<string> fileText, DateTime targetDate, CancellationToken cToken);
    Task PostFileAsync(DiscordTableMessage table, CancellationToken cToken);

    Task<List<ulong>> PostTableAsync(DiscordTableMessage table, DateTime targetDate, CancellationToken cToken);
    Task<List<ulong>> PostTableAsync(DiscordChannel channel, string tableHeading, List<List<string>> bodyData, DateTime targetDate, CancellationToken cToken);

    Task SendIpoFirstPassAlertsAsync(List<IpoFirstPassAlert> alertList, CancellationToken cToken);
    Task SendIpoSecondPassAlertsAsync(List<IpoSecondPassAlert> alertList, CancellationToken cToken);
}

public class DiscordMessager : IDiscordMessager
{
    private readonly IConsoleWriter _con;
    private readonly StonkBotDbContext _db;
    private readonly TargetLog _logWindow;
    private readonly DiscordConfig _config;
    
    public DiscordMessager(IConsoleWriter con, StonkBotDbContext db, IOptions<DiscordConfig> config)
    {
        _con = con;
        _db = db;
        _logWindow = TargetLog.ActionRunner;
        _config = config.Value;
    }

    private string GetWebhookUrl(DiscordChannel channel)
    {
        var url = channel switch
        {
            DiscordChannel.IpoWatch => _config.IpoWebhook,
            DiscordChannel.VolAlert => _config.VolumeAlertWebhook,
            DiscordChannel.VolAlert2 => _config.VolumeAlertWebhook,
            DiscordChannel.UpperShadow => _config.UpperShadowWebhook,
            DiscordChannel.FourHand => _config.FourHandWebhook,
            DiscordChannel.EarningsReport => _config.EarningsReportWebhook,
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

    public async Task PostFileAsync(DiscordTableMessage table, CancellationToken cToken)
    {
        var body = new StringBuilder();

        foreach (var row in table.Data)
        {
            foreach (var cell in row)
            {
                body.Append(cell);
                body.Append(",");
            }

            body.AppendLine();
        }

        var webhookUrl = GetWebhookUrl(table.Channel);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(body.ToString()));
        using var client = new DiscordWebhookClient(webhookUrl);

        var messageId = await client.SendFileAsync(stream, $"{table.Title}.csv", $"{table.Title}");

        await _db.DiscordMessageRecords.AddAsync(new DiscordMessageRecord
        {
            MessageId = messageId,
            DateTime = DateTime.Now.SbDate(),
            Channel = table.Channel.ToString()
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

    public async Task<List<ulong>> PostTableAsync(DiscordTableMessage table, DateTime targetDate, CancellationToken cToken)
    {
        var messages = await PostTableAsync(table.Channel, table.Title, table.Data, targetDate, cToken);

        return messages;
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
        
        // Create raw text table for discord
        const string codeBlock = "```";
        const string divider = "  |  ";
        const string newLine = "\r\n";
        var body = new StringBuilder();
        var line = new StringBuilder();
        body.AppendLine(codeBlock);
        body.AppendLine($"{tableHeading} - Date: [{targetDate.SbDateString()}]");
        foreach (var row in bodyData)
        {
            for (var i = 0; i < columnCount; i++)
            {
                var paddedString = row[i].PadLeft(maxWidths[i]);

                switch (i)
                {
                    case 0:
                        line.Append(paddedString);
                        break;

                    case var lastIndex when lastIndex == columnCount - 1:
                        line.Append(divider + paddedString + newLine);
                        break;

                    default:
                        line.Append(divider + paddedString);
                        break;
                }
            }

            // If line pushes us over 2000, send what we've got and start a new table
            if (body.Length + line.Length >= 2000)
            {
                body.Append(codeBlock);
                messageId = await client.SendMessageAsync(body.ToString());
                sentMessages.Add(messageId);

                body.Clear()
                    .AppendLine(codeBlock)
                    .AppendLine(line.ToString());
            }

            body.AppendLine(line.ToString());
            line.Clear();
            
        }
        body.Append(codeBlock);
        
        messageId = await client.SendMessageAsync(body.ToString());
        sentMessages.Add(messageId);

        return sentMessages;
    }

    public async Task SendIpoFirstPassAlertsAsync(List<IpoFirstPassAlert> alertList, CancellationToken cToken)
    {
        var alertDay = (alertList.FirstOrDefault()!).TodayDate;
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
        if (messages.Count == 0)
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
        var alertDay = (alertList.FirstOrDefault()!).TargetDate;
        var alertedSymbols = alertList
            .Select(x => x.Symbol)
            .ToList();

        var bodyData = new List<List<string>> { new() { "SYMBOL", "DAYS SATISFIED", "FIRST CHECK DATE CLOSE", "IPO DAY LOW", "TODAY CLOSE", "OPENINGDAYOPEN" } };
        alertList.ForEach(x => bodyData.Add(new List<string> { x.Symbol, x.DaysSatisfied.ToString(), $"{x.FirstCheckDate.SbDateString()}Close:[{x.CheckDayClose}]", x.OpeningDayLow, x.TodayClose, x.OpeningDayOpen }));
        var messages = await PostTableAsync(DiscordChannel.IpoWatch, "IPO Alerts 2ndPass", bodyData, alertDay, cToken);
        if (messages.Count == 0)
            return;

        var messageRecord = new DiscordMessageRecord
        {
            MessageId = messages.First(),
            Channel = DiscordChannel.IpoWatch.ToString(),
            DateTime = alertDay,
            Type = AlertType.IpoSecondPassTable
        };
        await _db.DiscordMessageRecords.AddAsync(messageRecord, cToken);

        await _db.IpoListings
            .Where(x => alertedSymbols.Contains(x.Symbol))
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.LastSecondPassDate, p => alertDay), cToken);
        await _db.SbSaveChangesAsync(cToken);
        alertedSymbols.ForEach(x => _con.WriteLog(MessageSeverity.Info, _logWindow, $"Added 'SecondPass' discordMessageRecords for watched Ipo:{x}"));




        /*var f1MaxLen = alertList.Select(x => x.FirstCheckDate.SbDateString()).MaxBy(x => x.Length)!.Length;
        var f2MaxLen = alertList.Select(x => x.CheckDayClose).MaxBy(x => x.Length)!.Length;
        var f3MaxLen = alertList.Select(x => x.OpeningDayLow).MaxBy(x => x.Length)!.Length;
        var f4MaxLen = alertList.Select(x => x.TodayClose).MaxBy(x => x.Length)!.Length;
        var f5MaxLen = alertList.Select(x => x.OpeningDayOpen).MaxBy(x => x.Length)!.Length;



        var msgBody = $"```\r\nIPO Alerts 2ndPass - Date: [{alertDay.SbDateString()}]\r\n";
        alertList.ForEach(x => msgBody += 
            //$"{x.Symbol,5}" +
            //$"  |  Days Satisfied: [{x.DaysSatisfied,3}]" +
            //$"  |  {x.FirstCheckDate.SbDateString().PadLeft(f1MaxLen)}Close:[{x.CheckDayClose.PadLeft(f2MaxLen)}]" +
            $"  |  IpoDayLow:[{x.OpeningDayLow.PadLeft(f3MaxLen)}]" +
            $"  |  TodayClose:[{x.TodayClose.PadLeft(f4MaxLen)}]" +
            $"  |  OpeningDayOpen:[{x.OpeningDayOpen.PadLeft(f5MaxLen)}]\r\n");
        msgBody += "```";

        await client.SendMessageAsync(msgBody);
        await dbIpos
            .Where(x => alertedSymbols.Contains(x.Symbol))
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.LastSecondPassDate, p => alertDay), cToken);
        await _db.SbSaveChangesAsync(cToken);
        alertedSymbols.ForEach(x => _con.WriteLog(MessageSeverity.Info, _logWindow, $"Sent 'SecondPass' DiscordAlert for watched Ipo:{x}"));*/
    }
}