namespace StonkBot.Services.CharlesSchwab.APIClient.Models;

public class HistoricalDataResponse
{
    public List<Candle> candles { get; set; } = new();
    public string symbol { get; set; } = null!;
    public bool empty { get; set; }
}

public class Candle
{
    public decimal open { get; set; }
    public decimal high { get; set; }
    public decimal low { get; set; }
    public decimal close { get; set; }
    public decimal volume { get; set; }
    public long datetime { get; set; }

    public DateTime GoodDate => DateTimeOffset.FromUnixTimeMilliseconds(datetime).Date;
    public DateTime GoodDateTime => DateTimeOffset.FromUnixTimeMilliseconds(datetime).DateTime;
}