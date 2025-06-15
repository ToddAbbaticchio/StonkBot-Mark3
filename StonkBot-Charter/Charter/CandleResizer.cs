using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using StonkBot.Data.Entities;
using StonkBotChartoMatic.Charter.Extensions;

namespace StonkBotChartoMatic.Charter;

internal class CandleResizer
{
    public static List<EsCandle> SetSize(List<EsCandle> esCandles, int candleLen)
    {
        return new List<EsCandle>();
    }
    
    public static List<DataTableRowV1> SetSize(DataTable dataTable, int candleLen)
    {
        var rows = dataTable.Rows;
        var currTableRow = 0;
        var combinedCandles = new List<DataTableRowV1>();
        while (currTableRow < rows.Count)
        {
            var candleStart = new DataTableRowV1(rows[currTableRow]);
            var averageVolume = Convert.ToInt32(candleStart.Volume);
            var range = dataTable.AsEnumerable().Skip(currTableRow).Take(candleLen);
            foreach (var row in range)
            {
                var thisRow = new DataTableRowV1(row);
                if (thisRow.ChartTime > candleStart.ChartTime!.Value.AddMinutes(candleLen)) break;

                if (thisRow.Low < candleStart.Low) candleStart.Low = thisRow.Low;
                if (thisRow.High > candleStart.High) candleStart.High = thisRow.High;
                candleStart.Close = thisRow.Close;
                averageVolume += Convert.ToInt32(thisRow.Volume);
            }
                
            candleStart.Volume = averageVolume / candleLen;
            combinedCandles.Add(candleStart);
            currTableRow += candleLen;
        }

        return combinedCandles;
    }

    public static List<DataTableRowV2> SetSizeV2(DataTable dataTable, int candleLen)
    {
        var rows = dataTable.Rows;
        var currTableRow = 0;
        var combinedCandles = new List<DataTableRowV2>();
        while (currTableRow < rows.Count)
        {
            var candleStart = new DataTableRowV2(rows[currTableRow]);
            var averageVolume = Convert.ToInt32(candleStart.Volume);
            var range = dataTable.AsEnumerable().Skip(currTableRow).Take(candleLen);
            foreach (var row in range)
            {
                var thisRow = new DataTableRowV2(row);
                if (thisRow.ChartTime > candleStart.ChartTime!.Value.AddMinutes(candleLen))
                    break;

                if (thisRow.Low < candleStart.Low) candleStart.Low = thisRow.Low;
                if (thisRow.High > candleStart.High) candleStart.High = thisRow.High;
                candleStart.Close = thisRow.Close;
                averageVolume += Convert.ToInt32(thisRow.Volume);
                if (thisRow.TPrice is not null)
                {
                    candleStart.TExecTime = thisRow.TExecTime;
                    candleStart.TPrice = thisRow.TPrice;
                    candleStart.TSide = thisRow.TSide;
                    candleStart.TPosEffect = thisRow.TPosEffect;
                }
            }

            candleStart.Volume = averageVolume / candleLen;
            combinedCandles.Add(candleStart);
            currTableRow += candleLen;
        }

        return combinedCandles;
    }
}