using Microsoft.AspNetCore.Mvc;
using StonkBot.Data.Entities;
using StonkBot.Data.Enums;
using StonkBot.Data;

namespace StonkBot_FE.Controllers;
[ApiController]
public class WeatherForecastController : ControllerBase
{
    private readonly IStonkBotCharterDb _db;

    public WeatherForecastController(IStonkBotCharterDb db)
    {
        _db = db;
    }

    [Route("weatherforecast")]
    [HttpGet]
    public IEnumerable<ReturnObject> Get()
    {
        Console.WriteLine("Starting weatherforecast Controller...");
        var Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };

        return Enumerable.Range(1, 5).Select(index => new ReturnObject
        {
            Date = DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = Summaries[Random.Shared.Next(Summaries.Length)]
        })
        .ToArray();
    }
}
