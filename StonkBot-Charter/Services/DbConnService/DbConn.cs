using StonkBot.Data;
using StonkBot.Data.Enums;
using StonkBot.Extensions;
using StonkBotChartoMatic.Charter;
using System;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace StonkBotChartoMatic.Services.DbConnService;

public interface IDbConn
{
    Task<DataTable> esCandleQuery(DateTime selectedDate, SbCharterMarket selectedMarket, CancellationToken cToken);
    Task<DataTable> historyESQuery(DateTime endDate, int range, CancellationToken cToken);
    Task<DataTable> targetZoneQuery(DateTime endDate, int range, CancellationToken cToken);
}

public class DbConn : IDbConn
{
    private readonly IStonkBotCharterDb _db;

    public DbConn(IStonkBotCharterDb db)
    {
        _db = db;
    }

    public async Task<DataTable> esCandleQuery(DateTime selectedDate, SbCharterMarket selectedMarket, CancellationToken cToken)
    {
        var hData = await _db.GetEsCandles(selectedDate, selectedMarket, cToken);
        return hData.ToDataTable();
    }

    public async Task<DataTable> historyESQuery(DateTime endDate, int range, CancellationToken cToken)
    {
        var start = endDate.GetPrevTradeDays(range).Order().First();
        var hData = await _db.GetEsCandleRangeAsync(start, endDate, cToken);
        return hData.ToDataTable();
    }

    public async Task<DataTable> targetZoneQuery(DateTime endDate, int range, CancellationToken cToken)
    {
        var start = endDate.GetPrevTradeDays(range).Order().First();
        var hData = await _db.GetTargetZoneCandles(start, endDate, cToken);
        return hData.ToDataTable();
    }
}