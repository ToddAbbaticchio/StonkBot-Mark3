using System;
using System.Collections.Generic;
using System.Data;
using System.Windows.Documents;
using StonkBot.Data.Enums;

namespace StonkBotChartoMatic.Charter.Extensions;

public class DataTableRowV1
{
    public DateTime? ChartTime { get; set; }
    public decimal Low { get; set; }
    public decimal High { get; set; }
    public decimal Open { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }

    public DataTableRowV1(DataRow row)
    {
        ChartTime = (DateTime)row["ChartTime"];
        Low = (decimal)row["Low"];
        High = (decimal)row["High"];
        Open = (decimal)row["Open"];
        Close = (decimal)row["Close"];
        Volume = (decimal)row["Volume"];
    }
}

public class DataTableRowV2
{
    public DateTime? ChartTime { get; set; }
    public decimal Low { get; set; }
    public decimal High { get; set; }
    public decimal Open { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public DateTime? TExecTime { get; set; }
    public string? TPrice { get; set; }
    public string? TSide { get; set; }
    public string? TPosEffect { get; set; }

    public DataTableRowV2(DataRow row)
    {
        ChartTime = (DateTime)row["ChartTime"];
        Low = (decimal)row["Low"];
        High = (decimal)row["High"];
        Open = (decimal)row["Open"];
        Close = (decimal)row["Close"];
        Volume = (decimal)row["Volume"];
        TExecTime = row.IsNull("tExecTime") ? null : (DateTime)row["tExecTime"];
        TPrice = row.IsNull("tPrice") ? null : (string)row["tPrice"];
        TSide = row.IsNull("tSide") ? null : (string)row["tSide"];
        TPosEffect = row.IsNull("tPosEffect") ? null : (string)row["tPosEffect"];
    }
}

/*public class DataTableRowTradeStation
{
    public DateTime? ChartTime { get; set; }
    public decimal Low { get; set; }
    public decimal High { get; set; }
    public decimal Open { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
    public DateTime? TExecTime { get; set; }
    public decimal? TPrice { get; set; }
    public string TSide { get; set; }
    public string TPosEffect { get; set; }

    public DataTableRowTradeStation(DataRow row)
    {
        // setup
        var chartTime = Convert.ToDateTime(row["charttime"]);
        if (row.IsNull("tExecTime")) TExecTime = null;
        else TExecTime = Convert.ToDateTime(row["tExecTime"]);
        if (row.IsNull("tPrice")) TPrice = null;
        else TPrice = Convert.Todecimal(row["tPrice"]);


        // apply values
        ChartTime = chartTime;
        Low = Convert.Todecimal(row["low"]);
        High = Convert.Todecimal(row["high"]);
        Open = Convert.Todecimal(row["open"]);
        Close = Convert.Todecimal(row["close"]);
        Volume = Convert.Todecimal(row["volume"]);
        TSide = row["tSide"].ToString();
        TPosEffect = row["tPosEffect"].ToString();
    }
}*/

public class HistoryESRow
{
    public decimal? Open { get; set; }
    public decimal? Close { get; set; }
    public decimal? Low { get; set; }
    public decimal? High { get; set; }
    public decimal? Volume { get; set; }
    public SbCharterMarket Market { get; set; }
    public DateTime? Date { get; set; }
    public decimal? Absflux { get; set; }
    public decimal? Dayflux { get; set; }
    public decimal? Nightflux { get; set; }
    public decimal? Abslow { get; set; }
    public decimal? Abshigh { get; set; }

    public HistoryESRow(DataRow row)
    {
        Open = row.IsNull("Open") ? null : (decimal)row["Open"];
        Close = row.IsNull("Close") ? null : (decimal)row["Close"];
        Low = row.IsNull("Low") ? null : (decimal)row["Low"];
        High = row.IsNull("High") ? null : (decimal)row["High"];
        Volume = row.IsNull("Volume") ? null : (decimal)row["Volume"];
        Market = (SbCharterMarket)row["Market"];
        Date = row.IsNull("Date") ? null : (DateTime)row["Date"];
        Absflux = row.IsNull("Absflux") ? null : (decimal)row["Absflux"];
        Dayflux = row.IsNull("Dayflux") ? null : (decimal)row["Dayflux"];
        Nightflux = row.IsNull("Nightflux") ? null : (decimal)row["Nightflux"];
        Abslow = row.IsNull("Abslow") ? null : (decimal)row["Abslow"];
        Abshigh = row.IsNull("Abshigh") ? null : (decimal)row["Abshigh"];
    }
}