#pragma warning disable IDE1006
#pragma warning disable CS8618

using Newtonsoft.Json;
using StonkBot.Extensions;

namespace StonkBot.Services.CharlesSchwab.StreamingClient.Models;

public interface ISignal
{
    public long timestamp { get; set; }
    public string symbol { get; set; }
    public DateTime TimeStamp { get; }
}

[Serializable]
public struct HeartbeatSignal
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
public enum BookOptions
{
    LISTED_BOOK,
    NASDAQ_BOOK,
    OPTIONS_BOOK,
    FUTURES_BOOK,
    FOREX_BOOK,
    FUTURES_OPTIONS_BOOK,
}


[Serializable]
public struct BookSignal : ISignal
{
    public long timestamp { get; set; }
    public string symbol { get; set; }
    public BookOptions id;
    public BookLevel[] bids;
    public BookLevel[] asks;
    public DateTime TimeStamp
    {
        get
        {
            return timestamp.SbDate();
        }
    }
}

[Serializable]
public struct BookLevel
{
    [JsonProperty("0")]
    public decimal price;
    [JsonProperty("1")]
    public decimal quantity;
}

[Serializable]
public struct QuoteSignal : ISignal
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
public struct TimeSaleSignal : ISignal
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
public struct ChartSignal : ISignal
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
public enum ChartSubs
{
    CHART_EQUITY,
    CHART_OPTIONS,
    CHART_FUTURES,
}

[Serializable]
public enum TimeSaleServices
{
    TIMESALE_EQUITY,
    TIMESALE_FOREX,
    TIMESALE_FUTURES,
    TIMESALE_OPTIONS,
}

[Serializable]
public enum QOSLevels
{
    EXPRESS,
    REALTIME,
    FAST,
    MODERATE,
    DELAYED,
    SLOW,
}

[Serializable]
public class RealtimeRequest
{
    public string service { get; set; }
    public string command { get; set; }
    public int requestid { get; set; }
    public string account { get; set; }
    public string source { get; set; }
    public object parameters { get; set; }
}

[Serializable]
public class RealtimeRequestContainer
{
    public RealtimeRequest[] requests { get; set; }
}

[Serializable]
public class RealtimeResponseContainer
{
    public RealtimeResponse[] response { get; set; }
}

[Serializable]
public class RealtimeResponse
{
    public string service { get; set; }
    public string requestid { get; set; }
    public string command { get; set; }
    public long timestamp { get; set; }
    public RealtimeContent content { get; set; }
    public DateTime TimeStamp
    {
        get
        {
            return timestamp.SbDate();
        }
    }
}

[Serializable]
public class RealtimeContent
{
    public int code { get; set; }
    public string msg { get; set; }
}