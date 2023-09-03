namespace StonkBot.Services.TDAmeritrade.APIClient.Models;

public class HistoricalDataResponse
{
    public List<HrCandle> candles { get; set; } = new();
    public string symbol { get; set; } = null!;
    public bool empty { get; set; }
}

public class HrCandle
{
    public decimal open { get; set; }
    public decimal high { get; set; }
    public decimal low { get; set; }
    public decimal close { get; set; }
    public decimal volume { get; set; }
    public long datetime { get; set; }
    public DateTime GoodDateTime { get; set; }
}