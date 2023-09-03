using StonkBot.Data;
using StonkBot.Services.ConsoleWriter;

namespace StonkBot.MarketPatterns;
public partial interface IMarketPatternMatcher
{
}

public partial class MarketPatternMatcher : IMarketPatternMatcher
{
    private IStonkBotDb _db;
    private readonly IConsoleWriter _con;

    public MarketPatternMatcher(IStonkBotDb db, IConsoleWriter con)
    {
        _db = db;
        _con = con;
    }
}