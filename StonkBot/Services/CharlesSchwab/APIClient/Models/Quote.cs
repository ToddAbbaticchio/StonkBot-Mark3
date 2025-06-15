using System.Text.Json.Serialization;
using System.Text.Json;

public class QuoteWrapper
{
    [JsonExtensionData]
    public Dictionary<string, JsonElement>? Data { get; set; }

    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNameCaseInsensitive = true,
        NumberHandling = JsonNumberHandling.AllowReadingFromString
    };

    public object? Result
    {
        get
        {
            if (Data == null || Data.Count == 0)
                return null;

            if (Data.Count == 1)
                return DeserializeSingleQuote();
            else
                return DeserializeMultipleQuotes();
        }
    }

    private Quote? DeserializeSingleQuote()
    {
        return JsonSerializer.Deserialize<Quote>(Data!.First().Value.GetRawText(), JsonOptions);
    }

    private List<Quote> DeserializeMultipleQuotes()
    {
        var list = new List<Quote>();
        foreach (var item in Data!)
        {
            var quote = JsonSerializer.Deserialize<Quote>(item.Value.GetRawText(), JsonOptions);
            if (quote == null)
                continue;
            list.Add(quote);
        }
        return list;
    }
}

public class Quote
{
    [JsonPropertyName("assetMainType")]
    public string? AssetMainType { get; set; }

    [JsonPropertyName("assetSubType")]
    public string? AssetSubType { get; set; }

    [JsonPropertyName("quoteType")]
    public string? QuoteType { get; set; }

    [JsonPropertyName("realtime")]
    public bool Realtime { get; set; }

    [JsonPropertyName("ssid")]
    public long Ssid { get; set; }

    [JsonPropertyName("extended")]
    public ExtendedData ExtendedData { get; set; } = null!;

    [JsonPropertyName("fundamental")]
    public FundamentalData? Fundamental { get; set; }

    [JsonPropertyName("quote")]
    public QuoteData QuoteData { get; set; } = null!;

    [JsonPropertyName("reference")]
    public ReferenceData? Reference { get; set; }

    [JsonPropertyName("regular")]
    public RegularData RegularData { get; set; } = null!;

    public string? symbol { get; set; }
    public decimal openPrice => QuoteData.OpenPrice;
    public decimal lowPrice => QuoteData.LowPrice;
    public decimal highPrice => QuoteData.HighPrice;
    public decimal regularMarketLastPrice => RegularData.RegularMarketLastPrice != 0 ? RegularData.RegularMarketLastPrice : QuoteData.ClosePrice;
    public decimal totalVolume => QuoteData.TotalVolume;
}

public class ExtendedData
{
    [JsonPropertyName("askPrice")]
    public decimal AskPrice { get; set; }

    [JsonPropertyName("askSize")]
    public int AskSize { get; set; }

    [JsonPropertyName("bidPrice")]
    public decimal BidPrice { get; set; }

    [JsonPropertyName("bidSize")]
    public int BidSize { get; set; }

    [JsonPropertyName("lastPrice")]
    public decimal LastPrice { get; set; }

    [JsonPropertyName("lastSize")]
    public int LastSize { get; set; }

    [JsonPropertyName("mark")]
    public decimal Mark { get; set; }

    [JsonPropertyName("quoteTime")]
    public long QuoteTime { get; set; }

    [JsonPropertyName("totalVolume")]
    public int TotalVolume { get; set; }

    [JsonPropertyName("tradeTime")]
    public long TradeTime { get; set; }
}

public class FundamentalData
{
    [JsonPropertyName("avg10DaysVolume")]
    public decimal Avg10DaysVolume { get; set; }

    [JsonPropertyName("avg1YearVolume")]
    public decimal Avg1YearVolume { get; set; }

    [JsonPropertyName("declarationDate")]
    public string? DeclarationDate { get; set; }

    [JsonPropertyName("divAmount")]
    public decimal DivAmount { get; set; }

    [JsonPropertyName("divExDate")]
    public string? DivExDate { get; set; }

    [JsonPropertyName("divFreq")]
    public int DivFreq { get; set; }

    [JsonPropertyName("divPayAmount")]
    public decimal DivPayAmount { get; set; }

    [JsonPropertyName("divPayDate")]
    public string? DivPayDate { get; set; }

    [JsonPropertyName("divYield")]
    public decimal DivYield { get; set; }

    [JsonPropertyName("eps")]
    public decimal Eps { get; set; }

    [JsonPropertyName("fundLeverageFactor")]
    public decimal FundLeverageFactor { get; set; }

    [JsonPropertyName("lastEarningsDate")]
    public string? LastEarningsDate { get; set; }

    [JsonPropertyName("nextDivExDate")]
    public string? NextDivExDate { get; set; }

    [JsonPropertyName("nextDivPayDate")]
    public string? NextDivPayDate { get; set; }

    [JsonPropertyName("peRatio")]
    public decimal PeRatio { get; set; }

}

public class QuoteData
{
    [JsonPropertyName("52WeekHigh")]
    public decimal _52WeekHigh { get; set; }

    [JsonPropertyName("52WeekLow")]
    public decimal _52WeekLow { get; set; }

    [JsonPropertyName("askMICId")]
    public string? AskMICId { get; set; }

    [JsonPropertyName("askPrice")]
    public decimal AskPrice { get; set; }

    [JsonPropertyName("askSize")]
    public int AskSize { get; set; }

    [JsonPropertyName("askTime")]
    public long AskTime { get; set; }

    [JsonPropertyName("bidMICId")]
    public string? BidMICId { get; set; }

    [JsonPropertyName("bidPrice")]
    public decimal BidPrice { get; set; }

    [JsonPropertyName("bidSize")]
    public int BidSize { get; set; }

    [JsonPropertyName("bidTime")]
    public long BidTime { get; set; }

    [JsonPropertyName("closePrice")]
    public decimal ClosePrice { get; set; }

    [JsonPropertyName("highPrice")]
    public decimal HighPrice { get; set; }

    [JsonPropertyName("lastMICId")]
    public string? LastMICId { get; set; }

    [JsonPropertyName("lastPrice")]
    public decimal LastPrice { get; set; }

    [JsonPropertyName("lastSize")]
    public int LastSize { get; set; }

    [JsonPropertyName("lowPrice")]
    public decimal LowPrice { get; set; }

    [JsonPropertyName("mark")]
    public decimal Mark { get; set; }

    [JsonPropertyName("markChange")]
    public decimal MarkChange { get; set; }

    [JsonPropertyName("markPercentChange")]
    public decimal MarkPercentChange { get; set; }

    [JsonPropertyName("netChange")]
    public decimal NetChange { get; set; }

    [JsonPropertyName("netPercentChange")]
    public decimal NetPercentChange { get; set; }

    [JsonPropertyName("openPrice")]
    public decimal OpenPrice { get; set; }

    [JsonPropertyName("postMarketChange")]
    public decimal PostMarketChange { get; set; }

    [JsonPropertyName("postMarketPercentChange")]
    public decimal PostMarketPercentChange { get; set; }

    [JsonPropertyName("quoteTime")]
    public long QuoteTime { get; set; }

    [JsonPropertyName("securityStatus")]
    public string? SecurityStatus { get; set; }

    [JsonPropertyName("totalVolume")]
    public long TotalVolume { get; set; }

    [JsonPropertyName("tradeTime")]
    public long TradeTime { get; set; }
}

public class ReferenceData
{
    [JsonPropertyName("cusip")]
    public string? Cusip { get; set; }

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [JsonPropertyName("exchange")]
    public string? Exchange { get; set; }

    [JsonPropertyName("exchangeName")]
    public string? ExchangeName { get; set; }

    [JsonPropertyName("isHardToBorrow")]
    public bool IsHardToBorrow { get; set; }

    [JsonPropertyName("isShortable")]
    public bool IsShortable { get; set; }

    [JsonPropertyName("htbRate")]
    public decimal HtbRate { get; set; }
}

public class RegularData
{
    [JsonPropertyName("regularMarketLastPrice")]
    public decimal RegularMarketLastPrice { get; set; }

    [JsonPropertyName("regularMarketLastSize")]
    public long RegularMarketLastSize { get; set; }

    [JsonPropertyName("regularMarketNetChange")]
    public decimal RegularMarketNetChange { get; set; }

    [JsonPropertyName("regularMarketPercentChange")]
    public decimal RegularMarketPercentChange { get; set; }

    [JsonPropertyName("regularMarketTradeTime")]
    public long RegularMarketTradeTime { get; set; }
}
