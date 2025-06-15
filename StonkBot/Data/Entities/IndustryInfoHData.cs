using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StonkBot.Data.Entities;

public class IndustryInfoHData
{
    [Key][Column(Order = 1)] public string Symbol { get; set; } = null!;
    [Key][Column(Order = 2)] public DateTime Date { get; set; }

    public IndustryInfo IndustryInfo { get; set; } = null!;

    public decimal? Volume { get; set; }
    public decimal? RelativeVolume { get; set; }
    public decimal? EarningsPerShare { get; set; }
    public decimal? VolatilityW { get; set; }
    public decimal? VolatilityM { get; set; }
    public decimal? TotalRevenue { get; set; }
    public decimal? NetIncome { get; set; }
    public decimal? MarketCap { get; set; }
}