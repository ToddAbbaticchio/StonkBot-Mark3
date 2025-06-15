#pragma warning disable CS8618
#pragma warning disable IDE1006
namespace StonkBot.Services.CharlesSchwab._Models;


public class UserPrincipals
{
    public string userId { get; set; }
    public string userCdDomainId { get; set; }
    public string primaryAccountId { get; set; }
    public DateTime lastLoginTime { get; set; }
    public DateTime tokenExpirationTime { get; set; }
    public DateTime loginTime { get; set; }
    public string accessLevel { get; set; }
    public bool stalePassword { get; set; }
    public Streamerinfo streamerInfo { get; set; }
    public string professionalStatus { get; set; }
    public Quotes quotes { get; set; }
    public Streamersubscriptionkeys streamerSubscriptionKeys { get; set; }
    public Exchangeagreements exchangeAgreements { get; set; }
    public Account[] accounts { get; set; }
}

public class Streamerinfo
{
    public string streamerBinaryUrl { get; set; }
    public string streamerSocketUrl { get; set; }
    public string token { get; set; }
    public DateTime tokenTimestamp { get; set; }
    public string userGroup { get; set; }
    public string accessLevel { get; set; }
    public string acl { get; set; }
    public string appId { get; set; }
}

public class Quotes
{
    public bool isNyseDelayed { get; set; }
    public bool isNasdaqDelayed { get; set; }
    public bool isOpraDelayed { get; set; }
    public bool isAmexDelayed { get; set; }
    public bool isCmeDelayed { get; set; }
    public bool isIceDelayed { get; set; }
    public bool isForexDelayed { get; set; }
}

public class Streamersubscriptionkeys
{
    public Key[] keys { get; set; }
}

public class Key
{
    public string key { get; set; }
}

public class Exchangeagreements
{
    public string OPRA_EXCHANGE_AGREEMENT { get; set; }
    public string NASDAQ_EXCHANGE_AGREEMENT { get; set; }
    public string NYSE_EXCHANGE_AGREEMENT { get; set; }
}

public class Account
{
    public string accountId { get; set; }
    public string displayName { get; set; }
    public string accountCdDomainId { get; set; }
    public string company { get; set; }
    public string segment { get; set; }
    public string acl { get; set; }
    public Authorizations authorizations { get; set; }
}

public class Authorizations
{
    public bool apex { get; set; }
    public bool levelTwoQuotes { get; set; }
    public bool stockTrading { get; set; }
    public bool marginTrading { get; set; }
    public bool streamingNews { get; set; }
    public string optionTradingLevel { get; set; }
    public bool streamerAccess { get; set; }
    public bool advancedMargin { get; set; }
    public bool scottradeAccount { get; set; }
}