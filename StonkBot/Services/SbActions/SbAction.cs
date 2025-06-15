using StonkBot.Data;
using StonkBot.MarketPatterns;
using StonkBot.Services.CharlesSchwab.APIClient;
using StonkBot.Services.ConsoleWriter;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.DiscordService;
using StonkBot.Services.WebScrapeService;

namespace StonkBot.Services.SbActions;

public partial interface ISbAction { }

internal partial class SbAction : ISbAction
{
    private IStonkBotDb _db;
    private readonly IConsoleWriter _con;
    private readonly IWebScraper _webScraper;
    private readonly ISchwabApiClient _marketClient;
    private readonly TargetLog _targetLog;
    private readonly IDiscordMessager _discordClient;
    private readonly IMarketPatternMatcher _sbPattern;
    
    public SbAction(
        IStonkBotDb db,
        IConsoleWriter con,
        IWebScraper webScraper,
        ISchwabApiClient marketClient,
        IDiscordMessager discordClient,
        IMarketPatternMatcher sbPattern)
    {
        _db = db;
        _con = con;
        _webScraper = webScraper;
        _marketClient = marketClient;
        _targetLog = TargetLog.ActionRunner;
        _discordClient = discordClient;
        _sbPattern = sbPattern;
    }
}