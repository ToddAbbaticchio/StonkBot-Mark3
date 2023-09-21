namespace StonkBot;

public class Constants
{
    public const string LocalDbFilePath = @"redacted";
    public const string NetworkDbFilePath = @"redacted";
    public const string DbBackupFolderPath = @"redacted";
    public const int MaxDbBackupAge = 10;
    
    public const int ProgressTick = 100;
    
    public const string WebullScrapeUrl = "https://www.webull.com/quote/us/ipo?hl=en";
    public const string ErScrapeUrl = "https://www.tradingview.com/markets/stocks-usa/earnings/";
    public const string IpoScrapeUrl = "https://www.webull.com/quote/us/ipo?hl=en";

    public const string TDTokenUrl = "https://api.tdameritrade.com/v1/oauth2/token";
    public const string TDQuoteUrl = "https://api.tdameritrade.com/v1/marketdata/PLACEHOLDER/quotes";
    public const string TDMultiQuoteUrl = "https://api.tdameritrade.com/v1/marketdata/quotes?symbol=PLACEHOLDER";
    public const string TDHistoricalQuoteUrl = "https://api.tdameritrade.com/v1/marketdata/SYMBOL/pricehistory?periodType=PERIODTYPE&period=PERIOD&frequencyType=FREQUENCYTYPE&frequency=1";
    public const string TDAmeritradeUserPrincipalsUrl = "https://api.tdameritrade.com/v1/userprincipals?fields=streamerSubscriptionKeys,streamerConnectionInfo";
    public const string TDAmeritradeClientId = "redacted";

    public const string VolumeAlertWebhook = "redacted";
    public const string UpperShadowWebhook = "redacted";
    public const string FourHandWebhook = "redacted";
    public const string IpoWebhook = "redacted";
    public const string EarningsReportWebhook = "redacted";

    public const string TDRedirectUrl = "https://localhost:8080/";
    public const string TDAmeritradeBaseUrl = "https://api.tdameritrade.com/v1";
    public const string TDAmeritradeTokenPath = "oauth2/token";
    
    public static readonly List<DateTime> MarketHolidays = new()
    {
        new DateTime(2022, 1, 17, 0, 0, 0),
        new DateTime(2022, 2, 21, 0, 0, 0),
        new DateTime(2022, 4, 15, 0, 0, 0),
        new DateTime(2022, 5, 30, 0, 0, 0),
        new DateTime(2022, 6, 20, 0, 0, 0),
        new DateTime(2022, 7, 4, 0, 0, 0),
        new DateTime(2022, 9, 5, 0, 0, 0),
        new DateTime(2022, 11, 24, 0, 0, 0),
        new DateTime(2022, 12, 26, 0, 0, 0),
        new DateTime(2023, 1, 2, 0, 0, 0),
        new DateTime(2023, 1, 16, 0, 0, 0),
        new DateTime(2023, 2, 20, 0, 0, 0),
        new DateTime(2023, 4, 7, 0, 0, 0),
        new DateTime(2023, 5, 29, 0, 0, 0),
        new DateTime(2023, 6, 19, 0, 0, 0),
        new DateTime(2023, 7, 4, 0, 0, 0),
        new DateTime(2023, 9, 4, 0, 0, 0),
        new DateTime(2023, 11, 23, 0, 0, 0),
        new DateTime(2023, 12, 25, 0, 0, 0),
        new DateTime(2024, 1, 1, 0, 0, 0),
        new DateTime(2024, 1, 15, 0, 0, 0),
        new DateTime(2024, 2, 19, 0, 0, 0),
        new DateTime(2024, 3, 29, 0, 0, 0),
        new DateTime(2024, 5, 27, 0, 0, 0),
        new DateTime(2024, 6, 19, 0, 0, 0),
        new DateTime(2024, 7, 4, 0, 0, 0),
        new DateTime(2024, 9, 2, 0, 0, 0),
        new DateTime(2024, 11, 28, 0, 0, 0),
        new DateTime(2024, 12, 25, 0, 0, 0),
        new DateTime(2025, 1, 1, 0, 0, 0),
        new DateTime(2025, 1, 20, 0, 0, 0),
        new DateTime(2025, 2, 17, 0, 0, 0),
        new DateTime(2025, 4, 18, 0, 0, 0),
        new DateTime(2025, 5, 26, 0, 0, 0),
        new DateTime(2025, 6, 19, 0, 0, 0),
        new DateTime(2025, 7, 4, 0, 0, 0),
        new DateTime(2025, 9, 1, 0, 0, 0),
        new DateTime(2025, 11, 27, 0, 0, 0),
        new DateTime(2025, 12, 25, 0, 0, 0),
        new DateTime(2026, 1, 1, 0, 0, 0),
        new DateTime(2026, 1, 19, 0, 0, 0),
        new DateTime(2026, 2, 16, 0, 0, 0),
        new DateTime(2026, 4, 3, 0, 0, 0),
        new DateTime(2026, 5, 25, 0, 0, 0),
        new DateTime(2026, 6, 19, 0, 0, 0),
        new DateTime(2026, 7, 3, 0, 0, 0),
        new DateTime(2026, 9, 7, 0, 0, 0),
        new DateTime(2026, 11, 26, 0, 0, 0),
        new DateTime(2026, 12, 25, 0, 0, 0)
    };
}
