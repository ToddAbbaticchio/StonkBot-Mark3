using Discord.Webhook;
using Microsoft.EntityFrameworkCore;
using StonkBot.Data;
using StonkBot.Data.Entities;
using StonkBot.Extensions;
using StonkBot.Services.ConsoleWriter;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.DiscordService.Enums;
using StonkBot.Services.SbActions._Models;

namespace StonkBot.Services.DiscordService;

public interface IDiscordMessager
{
    Task SendAlertAsync(DiscordChannel channel, string message, CancellationToken cToken);
    Task SendFileAsync(DiscordChannel channel, List<string>? fileText, DateTime targetDate, CancellationToken cToken);
    Task SendIpoFirstPassAlertsAsync(List<IpoFirstPassAlert> alertList, CancellationToken cToken);
    Task SendIpoSecondPassAlertsAsync(List<IpoSecondPassAlert> alertList, CancellationToken cToken);
}

public class DiscordMessager : IDiscordMessager
{
    private readonly IConsoleWriter _con;
    private readonly StonkBotDbContext _db;
    private readonly TargetLog _logWindow;
    
    public DiscordMessager(IConsoleWriter con, StonkBotDbContext db)
    {
        _con = con;
        _db = db;
        _logWindow = TargetLog.ActionRunner;
    }

    private string GetWebhookUrl(DiscordChannel channel)
    {
        var url = channel switch
        {
            DiscordChannel.IpoWatch => Constants.IpoWebhook,
            DiscordChannel.VolAlert => Constants.VolumeAlertWebhook,
            DiscordChannel.VolAlert2 => Constants.VolumeAlertWebhook,
            DiscordChannel.UpperShadow => Constants.UpperShadowWebhook,
            DiscordChannel.FourHand => Constants.FourHandWebhook,
            DiscordChannel.EarningsReport => Constants.EarningsReportWebhook,
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
            Date = DateTime.Today.SbDate(),
            Channel = channel.ToString(),
        }, cToken);

        await _db.SbSaveChangesAsync(cToken);
    }

    public async Task SendFileAsync(DiscordChannel channel, List<string>? fileText, DateTime targetDate, CancellationToken cToken)
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
            Date = DateTime.Now.SbDate(),
            Channel = channel.ToString()
        }, cToken);
        await _db.SbSaveChangesAsync(cToken);
    }

    public async Task SendIpoFirstPassAlertsAsync(List<IpoFirstPassAlert> alertList, CancellationToken cToken)
    {
        var dbIpos = _db.IpoListings;
        var alertDay = (alertList.FirstOrDefault()!).TodayDate;
        var client = new DiscordWebhookClient(Constants.IpoWebhook);
        var alertedSymbols = alertList
            .Select(x => x.Symbol)
            .ToList();

        var f1MaxLen = alertList.Select(x => x.TodayClose).MaxBy(x => x.Length)!.Length;
        var f2MaxLen = alertList.Select(x => x.OpeningHigh).MaxBy(x => x.Length)!.Length;

        var msgBody = $"```\r\nIPO Alerts 1stPass - Date: [{alertDay.SbDateString()}]\r\n";
        alertList.ForEach(x => msgBody += 
            $"{x.Symbol,5}" +
            $"  |  Days Satisfied: [{x.DaysSatisfied,3}]" +
            $"  |  TodayClose:[{x.TodayClose.PadLeft(f1MaxLen)}]" +
            $"  |  OpeningDayHigh:[{x.OpeningHigh.PadLeft(f2MaxLen)}]\r\n");
        msgBody += "```";

        await client.SendMessageAsync(msgBody);
        await dbIpos
            .Where(x => alertedSymbols.Contains(x.Symbol))
            .ExecuteUpdateAsync(x => x.SetProperty(p => p.FirstPassDate, p => alertDay), cToken);
        await _db.SbSaveChangesAsync(cToken);

        alertedSymbols.ForEach(x => _con.WriteLog(MessageSeverity.Info, _logWindow, $"Added 'FirstPass' discordMessageRecords for watched Ipo:{x}"));
    }

    public async Task SendIpoSecondPassAlertsAsync(List<IpoSecondPassAlert> alertList, CancellationToken cToken)
    {
        var dbIpos = _db.IpoListings;
        var alertDay = (alertList.FirstOrDefault()!).TargetDate;
        var client = new DiscordWebhookClient(Constants.IpoWebhook);
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