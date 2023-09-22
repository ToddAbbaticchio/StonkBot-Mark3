using StonkBot.Data;
using StonkBot.MarketPatterns;
using StonkBot.Options;
using StonkBot.Services.ConsoleWriter;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.DiscordService;
using StonkBot.Services.TDAmeritrade.APIClient;
using StonkBot.Services.WebScrapeService;

namespace StonkBot.Services.SbActions;

public partial interface ISbAction
{

}

internal partial class SbAction : ISbAction
{
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private IStonkBotDb _db;
    private readonly IConsoleWriter _con;
    private readonly IWebScraper _webScraper;
    private readonly ITdaApiClient _tdaClient;
    private readonly TargetLog _targetLog;
    private readonly IDiscordMessager _discordClient;
    private readonly IMarketPatternMatcher _sbPattern;
    
    public SbAction(
        IStonkBotDb db,
        IConsoleWriter con,
        IWebScraper webScraper,
        ITdaApiClient tdaClient,
        IDiscordMessager discordClient,
        IMarketPatternMatcher sbPattern)
    {
        _db = db;
        _con = con;
        _webScraper = webScraper;
        _tdaClient = tdaClient;
        _targetLog = TargetLog.ActionRunner;
        _discordClient = discordClient;
        _sbPattern = sbPattern;
    }
}