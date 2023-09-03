namespace StonkBot;

public class Constants
{
    public const string LocalDbFilePath = @"E:\projects\stonkBot\Data\StonkBot_Mark3.db";
    public const string NetworkDbFilePath = @"Z:\Data\StonkBot_Mark3.db";
    public const string DbBackupFolderPath = @"E:\projects\stonkBot\Data\Backups";
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
    public const string TDAmeritradeClientId = "6WDWKAYRVCUTOAQKJ0EOD3KALA8VZ2XC@AMER.OAUTHAP";

    public const string VolumeAlertWebhook = "https://discord.com/api/webhooks/1067440103351402586/9CtVbUb0Wjahnxd0Q1nyQ28pcI3LZlgrzfOI4fzCrgLpTTjDtm5Qsx9fwHQsTm9r2L1W";
    public const string UpperShadowWebhook = "https://discord.com/api/webhooks/1067440506277204038/6n4d79tdTBZImyxtWb2DbUe-AaXvQ1z8MTDke3jlAuP4W_QnO5HR1AoRbTXIglObsMjF";
    public const string FourHandWebhook = "https://discord.com/api/webhooks/1067440667137151036/WzO1wi_iGmt5IsBUh8Kpb-KiegUMShVLe2mo1UHStQvoP0sR78xeFSIZ1SxBcM313bqH";
    public const string IpoWebhook = "https://discord.com/api/webhooks/1067440330770751520/7vsQ-srHAirtlsybyJwG6z3nZ4Ey5hiS633V_atHOXPXV9vFnvSqmPsvC0u3nteRUM17";
    public const string EarningsReportWebhook = "https://discord.com/api/webhooks/1104834260524879892/vmmlgwIeYZyBUAI2IvXCCriLtSV3MWDSksKck0fMeQGzh_-ClTrY7p6lFdnNUehKyoAu";

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