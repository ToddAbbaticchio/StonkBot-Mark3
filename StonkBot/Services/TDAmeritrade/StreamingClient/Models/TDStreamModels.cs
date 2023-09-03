// ReSharper disable InconsistentNaming
#pragma warning disable IDE1006
#pragma warning disable CS8618

using Newtonsoft.Json;
using StonkBot.Extensions;

namespace StonkBot.Services.TDAmeritrade.StreamingClient.Models;

public interface ISignal
{
    public long timestamp { get; set; }
    public string symbol { get; set; }
    public DateTime TimeStamp { get; }
}

[Serializable]
public struct TDHeartbeatSignal
{
    public long timestamp { get; set; }
    public DateTime TimeStamp
    {
        get
        {
            return timestamp.SbDateTime();
        }
    }
}

[Serializable]
public enum TDBookOptions
{
    LISTED_BOOK,
    NASDAQ_BOOK,
    OPTIONS_BOOK,
    FUTURES_BOOK,
    FOREX_BOOK,
    FUTURES_OPTIONS_BOOK,
}


[Serializable]
public struct TDBookSignal : ISignal
{
    public long timestamp { get; set; }
    public string symbol { get; set; }
    public TDBookOptions id;
    public TDBookLevel[] bids;
    public TDBookLevel[] asks;
    public DateTime TimeStamp
    {
        get
        {
            return timestamp.SbDate();
        }
    }
}

[Serializable]
public struct TDBookLevel
{
    [JsonProperty("0")]
    public decimal price;
    [JsonProperty("1")]
    public decimal quantity;
}

[Serializable]
public struct TDQuoteSignal : ISignal
{
    public long timestamp { get; set; }
    public string symbol { get; set; }


    public decimal bidprice;
    public decimal askprice;
    public decimal bidsize;
    public decimal asksize;
    public decimal lastprice;
    public decimal lastsize;
    public long totalvolume;
    public decimal openprice;
    public decimal closeprice;
    public decimal lowprice;
    public decimal highprice;
    public long tradetime;
    public long quotetime;
    public char bidid;
    public char askid;
    public char bidtick;
    public decimal volatility;
    public DateTime TimeStamp
    {
        get
        {
            return timestamp.SbDateTime();
        }
    }
    public DateTime QuoteTime
    {
        get
        {
            return quotetime.SbDateTime();
        }
    }
    public DateTime TradeTime
    {
        get
        {
            return tradetime.SbDateTime();
        }
    }
}

[Serializable]
public struct TDTimeSaleSignal : ISignal
{
    public long timestamp { get; set; }
    public string symbol { get; set; }
    public long sequence;
    public long tradetime;
    public decimal lastprice;
    public decimal lastsize;
    public long lastsequence;
    public DateTime TimeStamp
    {
        get
        {
            return timestamp.SbDateTime();
        }
    }
    public DateTime TradeTime
    {
        get
        {
            return tradetime.SbDateTime();
        }
    }
}

[Serializable]
public struct TDChartSignal : ISignal
{
    public long timestamp { get; set; }
    public string symbol { get; set; }
    public decimal openprice;
    public decimal highprice;
    public decimal lowprice;
    public decimal closeprice;
    public decimal volume;
    public long sequence;
    public long charttime;
    public int chartday;
    public DateTime TimeStamp
    {
        get
        {
            return timestamp.SbDateTime();
        }
    }

    public DateTime ChartTime
    {
        get
        {
            return charttime.SbDateTime();
        }
    }
    public int ChartIndex
    {
        get
        {
            return ChartTime.ToCandleIndex(1);
        }
    }
}

[Serializable]
public enum TDChartSubs
{
    CHART_EQUITY,
    CHART_OPTIONS,
    CHART_FUTURES,
}

[Serializable]
public enum TDTimeSaleServices
{
    TIMESALE_EQUITY,
    TIMESALE_FOREX,
    TIMESALE_FUTURES,
    TIMESALE_OPTIONS,
}

[Serializable]
public enum TDQOSLevels
{
    EXPRESS,
    REALTIME,
    FAST,
    MODERATE,
    DELAYED,
    SLOW,
}

[Serializable]
public class TDRealtimeRequest
{
    public string service { get; set; }
    public string command { get; set; }
    public int requestid { get; set; }
    public string account { get; set; }
    public string source { get; set; }
    public object parameters { get; set; }
}

[Serializable]
public class TDRealtimeRequestContainer
{
    public TDRealtimeRequest[] requests { get; set; }
}

[Serializable]
public class TDRealtimeResponseContainer
{
    public TDRealtimeResponse[] response { get; set; }
}

[Serializable]
public class TDRealtimeResponse
{
    public string service { get; set; }
    public string requestid { get; set; }
    public string command { get; set; }
    public long timestamp { get; set; }
    public TDRealtimeContent content { get; set; }
    public DateTime TimeStamp
    {
        get
        {
            return timestamp.SbDate();
        }
    }
}

[Serializable]
public class TDRealtimeContent
{
    public int code { get; set; }
    public string msg { get; set; }
}