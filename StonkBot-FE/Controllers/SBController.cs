using Microsoft.AspNetCore.Mvc;
using StonkBot.Data;
using StonkBot.Data.Entities;
using StonkBot.Data.Enums;

namespace StonkBot_FE.Controllers;

[ApiController]
public class SBController : ControllerBase
{
    private readonly IStonkBotCharterDb _db;

    public SBController(IStonkBotCharterDb db)
    {
        _db = db;
    }

    [Route("escandlechart")]
    [HttpGet]
    public async Task<IEnumerable<EsCandle>> Get(CancellationToken cToken)
    {
        var todayData = await _db.GetEsCandles(DateTime.Today.Date.AddDays(-2), SbCharterMarket.Both, cToken);
        var grab5 = todayData.Take(5).ToArray();

        return grab5;
    }
}
