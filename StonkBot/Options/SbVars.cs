using Microsoft.Extensions.Configuration;

namespace StonkBot.Options;

public class SbVars
{
    public string LocalDbFilePath { get; }
    public string NetworkDbFilePath { get; }
    public string DbBackupFolderPath { get; }
    public int MaxDbBackupAge { get; }
    public int ProgressTick { get; }
    public string ErScrapeUrl { get; }
    public string IpoScrapeUrl { get; }
    public string TdTokenUrl { get; }
    public string TdQuoteUrl { get; }
    public string TdMultiQuoteUrl { get; }
    public string TdHistoricalQuoteUrl { get; }
    public string TdAmeritradeUserPrincipalsUrl { get; }
    public string TdAmeritradeClientId { get; }
    public string TdRedirectUrl { get; }
    public string TdAmeritradeBaseUrl { get; }
    public string TdAmeritradeTokenPath { get; }
    public string VolumeAlertWebhook { get; }
    public string UpperShadowWebhook { get; }
    public string FourHandWebhook { get; }
    public string IpoWebhook { get; }
    public string EarningsReportWebhook { get; }

    public SbVars(IConfiguration config)
    {
        LocalDbFilePath = config.GetValue<string>("SbVars:LocalDbFilePath")!;
        NetworkDbFilePath = config.GetValue<string>("SbVars:NetworkDbFilePath")!;
        DbBackupFolderPath = config.GetValue<string>("SbVars:DbBackupFolderPath")!;
        MaxDbBackupAge = config.GetValue<int>("SbVars:MaxDbBackupAge")!;
        ProgressTick = config.GetValue<int>("SbVars:ProgressTick")!;
        ErScrapeUrl = config.GetValue<string>("SbVars:ErScrapeUrl")!;
        IpoScrapeUrl = config.GetValue<string>("SbVars:IpoScrapeUrl")!;
        TdTokenUrl = config.GetValue<string>("SbVars:TdTokenUrl")!;
        TdQuoteUrl = config.GetValue<string>("SbVars:TdQuoteUrl")!;
        TdMultiQuoteUrl = config.GetValue<string>("SbVars:TdMultiQuoteUrl")!;
        TdHistoricalQuoteUrl = config.GetValue<string>("SbVars:TdHistoricalQuoteUrl")!;
        TdAmeritradeUserPrincipalsUrl = config.GetValue<string>("SbVars:TdAmeritradeUserPrincipalsUrl")!;
        TdAmeritradeClientId = config.GetValue<string>("SbVars:TdAmeritradeClientId")!;
        TdRedirectUrl = config.GetValue<string>("SbVars:TdRedirectUrl")!;
        TdAmeritradeBaseUrl = config.GetValue<string>("SbVars:TdAmeritradeBaseUrl")!;
        TdAmeritradeTokenPath = config.GetValue<string>("SbVars:TdAmeritradeTokenPath")!;
        VolumeAlertWebhook = config.GetValue<string>("SbVars:VolumeAlertWebhook")!;
        UpperShadowWebhook = config.GetValue<string>("SbVars:UpperShadowWebhook")!;
        FourHandWebhook = config.GetValue<string>("SbVars:FourHandWebhook")!;
        IpoWebhook = config.GetValue<string>("SbVars:IpoWebhook")!;
        EarningsReportWebhook = config.GetValue<string>("SbVars:EarningsReportWebhook")!;
    }
}