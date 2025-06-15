using StonkBot.Services.SbActions._Enums;

namespace StonkBot.Services.SbActions._Models;

public class ActionSchedule
{
    public DateTime CreatedOn { get; set; } = DateTime.Today;
    public List<ActionInfo> ActionInfos { get; set; } = new List<ActionInfo>();

    public ActionSchedule()
    {
        ActionInfos.Add(new ActionInfo
        (
            method: "IpoScrape",
            startTime: "00:00:00",
            endTime: "23:59:59",
            interval: ActionInterval.Hour3
        ));

        ActionInfos.Add(new ActionInfo
        (
            method: "IndustryInfoScrape",
            startTime: "8:45:00 PM",
            endTime: "23:59:59",
            interval: ActionInterval.Daily
        ));

        ActionInfos.Add(new ActionInfo
        (
            method: "IpoCheck",
            startTime: "4:05:00 PM",
            endTime: "23:59:59",
            interval: ActionInterval.Hour1
        ));

        ActionInfos.Add(new ActionInfo
        (
            method: "EarningsReportCheck",
            startTime: "4:10:00 PM",
            endTime: "23:59:59",
            interval: ActionInterval.Hour1
        ));

        ActionInfos.Add(new ActionInfo
        (
            method: "GetMarketData",
            startTime: "4:05:00 PM",
            endTime: "23:59:59",
            interval: ActionInterval.Hour1
        ));

        ActionInfos.Add(new ActionInfo
        (
            method: "UpdateCalculatedFields",
            startTime: "8:15:00 PM",
            endTime: "23:59:59",
            interval: ActionInterval.Hour1
        ));

        ActionInfos.Add(new ActionInfo
        (
            method: "DiscordAlert",
            startTime: "8:30:00 PM",
            endTime: "23:59:59",
            interval: ActionInterval.Daily
        ));

        ActionInfos.Add(new ActionInfo
        (
            method: "DbBackupChecks",
            startTime: "7:30:00 PM",
            endTime: "7:59:59 PM",
            interval: ActionInterval.Min10
        ));

        ActionInfos.Add(new ActionInfo
        (
            method: "DailyEsCandles",
            startTime: "5:10:00 PM",
            endTime: "6:00:00 PM",
            interval: ActionInterval.Daily
        ));

        ActionInfos.Add(new ActionInfo
        (
            method: "DailyMostActive",
            startTime: "5:30:00 PM",
            endTime: "23:59:59",
            interval: ActionInterval.Hour1
        ));
    }
}