using StonkBot.Data.Entities;
using StonkBot.Extensions;
using StonkBot.Services.WebScrapeService.Models;

namespace StonkBot.Services.WebScrapeService.Extensions;

public static class WebScrapeExtensions
{
    public static (IndustryInfo, IndustryInfoHData) ToIndustryInfo(this DataItem x, string sector, string industry)
    {
        var iInfo = new IndustryInfo
        {
            Symbol = x.D[0].ToString()!,
            Sector = sector,
            Industry = industry,
            Category = x.D[8].ToCategory(),
        };

        var iInfoHData = new IndustryInfoHData
        {
            Symbol = x.D[0].ToString()!,
            Date = DateTime.Now.SbDate(),
            Volume = Convert.ToDecimal(x.D[1]),
            RelativeVolume = Convert.ToDecimal(x.D[2]),
            EarningsPerShare = Convert.ToDecimal(x.D[3]),
            VolatilityW = Convert.ToDecimal(x.D[4]),
            VolatilityM = Convert.ToDecimal(x.D[5]),
            TotalRevenue = Convert.ToDecimal(x.D[6]),
            NetIncome = Convert.ToDecimal(x.D[7]),
            MarketCap = Convert.ToDecimal(x.D[8])
        };

        return (iInfo, iInfoHData);
    }

    public static string ToCategory(this object x)
    {
        var marketCap = Convert.ToDecimal(x);
        return marketCap switch
        {
            >= 10_000_000_000m => "Large Cap",
            >= 2_000_000_000m => "Mid Cap",
            >= 300_000_000m => "Small Cap",
            _ => "Micro Cap"
        };
    }
}
