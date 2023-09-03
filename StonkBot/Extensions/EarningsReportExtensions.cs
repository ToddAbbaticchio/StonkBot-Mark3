using StonkBot.Data;
using StonkBot.Data.Entities;

namespace StonkBot.Extensions;

public static class EarningsReportExtensions
{
    public static async Task<string> IsWatched(this EarningsReport er)
    {
        await using var _db = new StonkBotDbContext();
        return _db.WatchedSymbols.Any(x => x.Symbol == er.Symbol) ? "WATCHED" : "";
    }
}