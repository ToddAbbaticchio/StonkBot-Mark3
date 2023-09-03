using Newtonsoft.Json;
// ReSharper disable InconsistentNaming
#pragma warning disable IDE1006

namespace StonkBot.Services.TDAmeritrade.APIClient.Models;

[JsonConverter(typeof(QuoteConverter))]
public class QuoteList
{
    public List<Quote>? Info { get; set; }
}

public class Quote
{
    public string? assetType { get; set; }
    public string? assetMadecimalype { get; set; }
    public string? cusip { get; set; }
    public string symbol { get; set; } = null!;
    public string? description { get; set; }
    public decimal bidPrice { get; set; }
    public decimal bidSize { get; set; }
    public string? bidId { get; set; }
    public decimal askPrice { get; set; }
    public decimal askSize { get; set; }
    public string? askId { get; set; }
    public decimal lastPrice { get; set; }
    public decimal lastSize { get; set; }
    public string? lastId { get; set; }
    public decimal openPrice { get; set; }
    public decimal highPrice { get; set; }
    public decimal lowPrice { get; set; }
    public string? bidTick { get; set; }
    public decimal closePrice { get; set; }
    public decimal netChange { get; set; }
    public decimal totalVolume { get; set; }
    public long quoteTimeInLong { get; set; }
    public long tradeTimeInLong { get; set; }
    public decimal mark { get; set; }
    public string? exchange { get; set; }
    public string? exchangeName { get; set; }
    public bool marginable { get; set; }
    public bool shortable { get; set; }
    public decimal volatility { get; set; }
    public decimal digits { get; set; }
    [JsonProperty("52WkHigh")] public decimal _52WkHigh { get; set; }
    [JsonProperty("52WkLow")] public decimal _52WkLow { get; set; }
    public decimal nAV { get; set; }
    public decimal peRatio { get; set; }
    public decimal divAmount { get; set; }
    public decimal divYield { get; set; }
    public string? divDate { get; set; }
    public string? securityStatus { get; set; }
    public decimal regularMarketLastPrice { get; set; }
    public decimal regularMarketLastSize { get; set; }
    public decimal regularMarketNetChange { get; set; }
    public long regularMarketTradeTimeInLong { get; set; }
    public decimal netPercentChangeInDouble { get; set; }
    public decimal markChangeInDouble { get; set; }
    public decimal markPercentChangeInDouble { get; set; }
    public decimal regularMarketPercentChangeInDouble { get; set; }
    public bool delayed { get; set; }
}