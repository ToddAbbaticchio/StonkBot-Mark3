using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using StonkBot.Services.CharlesSchwab.StreamingClient.Models;

#pragma warning disable CS8602
#pragma warning disable CS8601
#pragma warning disable CS8604

namespace StonkBot.Services.CharlesSchwab.StreamingClient;

public class StreamJsonProcessor
{
    public event Action<HeartbeatSignal> OnHeartbeatSignal = delegate { };
    public event Action<ChartSignal> OnChartSignal = delegate { };
    public event Action<QuoteSignal> OnQuoteSignal = delegate { };
    public event Action<TimeSaleSignal> OnTimeSaleSignal = delegate { };
    public event Action<BookSignal> OnBookSignal = delegate { };

    public void Parse(string json)
    {
        var job = JObject.Parse(json);

        if (job.ContainsKey("notify"))
        {
            var test = JsonConvert.DeserializeObject<HeartbeatSignal>(json);
            
            ParseHeartbeat(job["notify"].First.First.ToObject<long>());
        }
        else if (job.ContainsKey("data"))
        {
            var data = job["data"] as JArray;
            foreach (var item in data)
            {
                var service = item.Value<string>("service");
                var contents = item.Value<JArray>("content");
                var tmstamp = item["timestamp"].Value<long>();

                if (contents == null)
                    return;

                foreach (var content in contents.Children<JObject>())
                {
                    if (service == "QUOTE")
                    {
                        ParseQuote(tmstamp, content);
                    }
                    else if (service == "CHART_FUTURES")
                    {
                        ParseChartFutures(tmstamp, content);
                    }
                    else if (service == "CHART_EQUITY")
                    {
                        ParseChartEquity(tmstamp, content);
                    }
                    else if (service == "LISTED_BOOK" || service == "NASDAQ_BOOK" || service == "OPTIONS_BOOK")
                    {
                        ParseBook(tmstamp, content, service);
                    }
                    else if (service == "TIMESALE_EQUITY" || service == "TIMESALE_FUTURES" || service == "TIMESALE_FOREX" || service == "TIMESALE_OPTIONS")
                    {
                        ParseTimeSaleEquity(tmstamp, content);
                    }
                }
            }
        }
    }

    void ParseHeartbeat(long tmstamp)
    {
        var model = new HeartbeatSignal { timestamp = tmstamp };
        OnHeartbeatSignal(model);
    }

    void ParseBook(long tmstamp, JObject content, string service)
    {
        var model = new BookSignal
        {
            timestamp = tmstamp,
            id = (BookOptions)Enum.Parse(typeof(BookOptions), service)
        };

        foreach (var item in content)
        {
            switch (item.Key)
            {
                case "key":
                    model.symbol = item.Value.Value<string>();
                    break;
                //case "1":
                //    model.booktime = item.Value.Value<long>();
                //    break;
                case "2":
                    model.bids = (item.Value as JArray).ToObject<BookLevel[]>();
                    break;
                case "3":
                    model.asks = (item.Value as JArray).ToObject<BookLevel[]>();
                    break;
            }
        }
        OnBookSignal(model);
    }

    void ParseChartFutures(long tmstamp, JObject content)
    {
        var model = new ChartSignal { timestamp = tmstamp };
        foreach (var item in content)
        {
            switch (item.Key)
            {
                case "key":
                    model.symbol = item.Value.Value<string>();
                    break;
                case "seq":
                    model.sequence = item.Value.Value<long>();
                    break;
                case "1":
                    model.charttime = item.Value.Value<long>();
                    break;
                case "2":
                    model.openprice = item.Value.Value<decimal>();
                    break;
                case "3":
                    model.highprice = item.Value.Value<decimal>();
                    break;
                case "4":
                    model.lowprice = item.Value.Value<decimal>();
                    break;
                case "5":
                    model.closeprice = item.Value.Value<decimal>();
                    break;
                case "6":
                    model.volume = item.Value.Value<long>();
                    break;
            }
        }
        OnChartSignal(model);
    }

    void ParseChartEquity(long tmstamp, JObject content)
    {
        var model = new ChartSignal { timestamp = tmstamp };
        foreach (var item in content)
        {
            switch (item.Key)
            {
                case "key":
                    model.symbol = item.Value.Value<string>();
                    break;
                case "seq":
                    model.sequence = item.Value.Value<long>();
                    break;
                case "1":
                    model.openprice = item.Value.Value<decimal>();
                    break;
                case "2":
                    model.highprice = item.Value.Value<decimal>();
                    break;
                case "3":
                    model.lowprice = item.Value.Value<decimal>();
                    break;  
                case "4":
                    model.closeprice = item.Value.Value<decimal>();
                    break;
                case "5":
                    model.volume = item.Value.Value<decimal>();
                    break;
                case "6":
                    model.sequence = item.Value.Value<long>();
                    break;
                case "7":
                    model.charttime = item.Value.Value<long>();
                    break;
                case "8":
                    model.chartday = item.Value.Value<int>();
                    break;
            }
        }
        OnChartSignal(model);
    }

    void ParseTimeSaleEquity(long tmstamp, JObject content)
    {
        var model = new TimeSaleSignal { timestamp = tmstamp };
        foreach (var item in content)
        {
            switch (item.Key)
            {
                case "key":
                    model.symbol = item.Value.Value<string>();
                    break;
                case "seq":
                    model.sequence = item.Value.Value<long>();
                    break;
                case "1":
                    model.tradetime = item.Value.Value<long>();
                    break;
                case "2":
                    model.lastprice = item.Value.Value<decimal>();
                    break;
                case "3":
                    model.lastsize = item.Value.Value<decimal>();
                    break;
                case "4":
                    model.lastsequence = item.Value.Value<long>();
                    break;
            }
        }
        OnTimeSaleSignal(model);
    }

    void ParseQuote(long tmstamp, JObject content)
    {
        var model = new QuoteSignal { timestamp = tmstamp };
        foreach (var item in content)
        {
            switch (item.Key)
            {
                case "key":
                    model.symbol = item.Value.Value<string>();
                    break;
                case "1":
                    model.bidprice = item.Value.Value<decimal>();
                    break;
                case "2":
                    model.askprice = item.Value.Value<decimal>();
                    break;
                case "3":
                    model.lastprice = item.Value.Value<decimal>();
                    break;
                case "4":
                    model.bidsize = item.Value.Value<decimal>();
                    break;
                case "5":
                    model.asksize = item.Value.Value<decimal>();
                    break;
                case "6":
                    model.askid = item.Value.Value<char>();
                    break;
                case "7":
                    model.bidid = item.Value.Value<char>();
                    break;
                case "8":
                    model.totalvolume = item.Value.Value<long>();
                    break;
                case "9":
                    model.lastsize = item.Value.Value<decimal>();
                    break;
                case "10":
                    model.tradetime = item.Value.Value<long>();
                    break;
                case "11":
                    model.quotetime = item.Value.Value<long>();
                    break;
                case "12":
                    model.highprice = item.Value.Value<decimal>();
                    break;
                case "13":
                    model.lowprice = item.Value.Value<decimal>();
                    break;
                case "14":
                    model.bidtick = item.Value.Value<char>();
                    break;
                case "15":
                    model.closeprice = item.Value.Value<decimal>();
                    break;
                case "24":
                    model.volatility = item.Value.Value<decimal>();
                    break;
                case "28":
                    model.openprice = item.Value.Value<decimal>();
                    break;
            }
        }
        OnQuoteSignal(model);
    }
}