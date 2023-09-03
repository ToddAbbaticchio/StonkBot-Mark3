/*using StonkBotChartoMatic.Charter;
using StonkBotChartoMatic.Charter.Extensions;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;
using StonkBotChartoMatic.Services.FileUtilService.Models;
using static System.String;

namespace StonkBotChartoMatic.Services.ChartUtilService;

public interface IChartUtil
{
    Task DrawCandleChartWithTransactionPoints(List<TCandle> tCandles, int candleSize, CancellationToken cToken);

*//*    Task DrawCandleChart(List<DataTableRowV1> candleList, DataTable dataTable, Chart chart, Chart chart2, CancellationToken cToken);
    Task DrawFluxValueChart(DataTable dataTable, Chart chart, CancellationToken cToken);
    Task DrawESHighLowChart(DataTable dataTable, Chart chart, CancellationToken cToken);
    Task DrawTargetZoneCandleChart(List<DataTableRowV1> rowList, DataTable dataTable, Chart chart, Label textField, CancellationToken cToken);*//*
    //Task AddAnnotation(Chart chart, DataPoint dataPoint, string labelText, CancellationToken cToken);
}

public class ChartUtil : IChartUtil
{
    public Chart chart1 = CharterForm.mainChart;
    public Chart chart2 = CharterForm.subChart;
    
    public async Task DrawCandleChartWithTransactionPoints(List<TCandle> tCandles, int candleSize, CancellationToken cToken)
    {
        #region ChartSetup
        await SetChartSize(tCandles);
        

        chart1.Series.Clear();
        chart2.Series.Clear();

        // Chart1 Setup
        chart1.ChartAreas.First().Name = "Chart1Area";
        chart1.ChartAreas.First().AxisY.Minimum = chartSize.YMin;
        chart1.ChartAreas.First().AxisY.LabelStyle.Format = "0";
        chart1.ChartAreas.First().AxisX.LabelStyle.Format = "HH:mm";

        var cSeries = new Series();
        cSeries.Name = "CandleStickSeries";
        cSeries.ChartArea = "Chart1Area";
        cSeries.ChartType = SeriesChartType.Candlestick;
        cSeries.YValuesPerPoint = 4;
        cSeries.XValueType = ChartValueType.DateTime;
        cSeries.YValueType = ChartValueType.Double;
        chart1.Series.Add(cSeries);

        var tSeries = new Series();
        tSeries.Name = "TransactionSeries";
        tSeries.ChartArea = "Chart1Area";
        tSeries.ChartType = SeriesChartType.Point;
        tSeries.XValueType = ChartValueType.DateTime;
        tSeries.YValueType = ChartValueType.Double;
        tSeries.MarkerSize = 10;
        tSeries.MarkerBorderColor = Color.Black;
        tSeries.MarkerBorderWidth = 1;
        chart1.Series.Add(tSeries);

        // Chart2 Setup
        chart2.ChartAreas.First().Name = "Chart2Area";
        chart2.ChartAreas.First().AxisY.LabelStyle.Format = "0";
        chart2.ChartAreas.First().AxisX.LabelStyle.Format = "HH:mm";

        var vSeries = new Series();
        vSeries.Name = "VolumeSeries";
        vSeries.ChartArea = "Chart2Area";
        vSeries.ChartType = SeriesChartType.Column;
        vSeries.XValueType = ChartValueType.DateTime;
        vSeries.YValueType = ChartValueType.Double;
        chart2.Series.Add(vSeries);
        #endregion

        var candleList = CandleResizer.SetSizeV2(dataTable, candleSize);
        var tList = candleList.Where(x => x.TPrice != null);

        var candleCount = 0;
        foreach (var candle in candleList)
        {
            candleCount++;
            object[] candleInfo = { candle.Low, candle.High, candle.Open, candle.Close };
            var thisC = cSeries.Points.AddXY(candle.ChartTime, candleInfo);
            var thisV = vSeries.Points.AddXY(candle.ChartTime, candle.Volume);

            // Set C and V dataPoint colors
            if (candle.Open > candle.Close)
            {
                cSeries.Points[thisC].Color = Color.FromArgb(255, Color.OrangeRed);
                cSeries.Points[thisC].BackSecondaryColor = Color.FromArgb(255, Color.OrangeRed);
                vSeries.Points[thisV].Color = Color.FromArgb(255, Color.OrangeRed);
            }
            else
            {
                cSeries.Points[thisC].Color = Color.FromArgb(255, Color.ForestGreen);
                vSeries.Points[thisV].Color = Color.FromArgb(255, Color.ForestGreen);
            }

            // If we have a transaction, plot it
            if (candle.TPrice is null)
                continue;

            var pList = Array.ConvertAll(candle.TPrice.Split(","), decimal.Parse);
            var eList = candle.TPosEffect!.Split(",").ToList();
            for (var i = 0; i < pList.Length; i++)
            {
                var thisT = tSeries.Points.AddXY(candle.ChartTime, pList[i]);
                tSeries.Points[thisT].Color = GetTransactionPointColor(eList, i);
                tSeries.Points[thisT].ToolTip = GetTransactionTooltipText(candle);
            }
        }
    }



    *//*public async Task DrawCandleChart(List<DataTableRowV1> candleList, DataTable dataTable, Chart chart1, Chart chart2, CancellationToken cToken)
    {
        #region ChartSetup
        var chartSize = new CandleChartSizeInfo(dataTable);
        chart1.ChartAreas[0].AxisY.Minimum = chartSize.YMin;
        chart1.ChartAreas[0].AxisY.LabelStyle.Format = "0";
        chart1.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm";
        chart1.Series.Clear();

        chart2.ChartAreas[0].AxisY.LabelStyle.Format = "0";
        chart2.ChartAreas[0].AxisX.LabelStyle.Format = "HH:mm";
        chart2.Series.Clear();

        var cSeries = new Series();
        cSeries.ChartArea = "ChartArea1";
        cSeries.ChartType = SeriesChartType.Candlestick;
        cSeries.XValueType = ChartValueType.DateTime;
        cSeries.YValueType = ChartValueType.Double;
        cSeries.YValuesPerPoint = 4;

        var vSeries = new Series();
        vSeries.ChartArea = "ChartArea1";
        vSeries.ChartType = SeriesChartType.Column;
        vSeries.XValueType = ChartValueType.DateTime;
        vSeries.YValueType = ChartValueType.Double;
        #endregion

        foreach (var candle in candleList)
        {
            object[] candleInfo = { candle.Low, candle.High, candle.Open, candle.Close };
            var thisC = cSeries.Points.AddXY(candle.ChartTime, candleInfo);
            var thisV = vSeries.Points.AddXY(candle.ChartTime, candle.Volume);

            // Set C and V dataPoint colors
            if (candle.Open > candle.Close)
            {
                cSeries.Points[thisC].Color = Color.FromArgb(255, Color.OrangeRed);
                cSeries.Points[thisC].BackSecondaryColor = Color.FromArgb(255, Color.OrangeRed);
                vSeries.Points[thisV].Color = Color.FromArgb(255, Color.OrangeRed);
            }
            else
            {
                cSeries.Points[thisC].Color = Color.FromArgb(255, Color.ForestGreen);
                vSeries.Points[thisV].Color = Color.FromArgb(255, Color.ForestGreen);
            }

            // Set dataPoint tooltips ???
            //cNewPoint.LabelToolTip = $"Open: {open}{Environment.NewLine}Close: {close}{Environment.NewLine}Low: {low}{Environment.NewLine}High: {high}{Environment.NewLine}Time: {time}{Environment.NewLine}Volume: {volume}";
            //vNewPoint.LabelToolTip = $"Volume: {volume}{Environment.NewLine}Time: {time}";
        }

        //chart.DataBind();
        chart1.Series.Add(cSeries);
        chart2.Series.Add(vSeries);
    }

    public async Task DrawFluxValueChart(DataTable dataTable, Chart chart, CancellationToken cToken)
    {
        #region ChartSetup
        var chartSize = new FluxChartSizeInfo(dataTable);
        chart.ChartAreas[0].AxisY.Minimum = 0;
        chart.ChartAreas[0].AxisY.LabelStyle.Format = "0";
        chart.ChartAreas[0].AxisX.LabelStyle.Format = "MM/dd/yy";
        chart.ChartAreas[0].AxisX.Interval = 1;
        chart.Series.Clear();

        var fluxSeries = new Series();
        fluxSeries.ChartArea = "ChartArea1";
        fluxSeries.ChartType = SeriesChartType.Column;
        fluxSeries.XValueType = ChartValueType.DateTime;
        fluxSeries.YValueType = ChartValueType.Double;
        fluxSeries.Label = "#VALY";
        #endregion

        foreach (DataRow row in dataTable.Rows)
        {
            if (IsNullOrEmpty(row["absflux"].ToString()))
            {
                continue;
            }

            var rowInfo = new HistoryESRow(row);
            fluxSeries.Points.AddXY(rowInfo.Date, rowInfo.Absflux);
        }

        chart.Series.Add(fluxSeries);
    }

    public async Task DrawESHighLowChart(DataTable dataTable, Chart chart, CancellationToken cToken)
    {
        #region ChartSetup
        var chartSize = new ESHighLowSizeInfo(dataTable);
        chart.ChartAreas[0].AxisY.Minimum = 0;
        chart.ChartAreas[0].AxisY.LabelStyle.Format = "0";
        chart.ChartAreas[0].AxisX.LabelStyle.Format = "MM/dd/yy";
        chart.ChartAreas[0].AxisX.Interval = 1;
        chart.Series.Clear();

        var cSeries = new Series();
        cSeries.ChartArea = "ChartArea1";
        cSeries.ChartType = SeriesChartType.Column;
        cSeries.XValueType = ChartValueType.DateTime;
        cSeries.YValueType = ChartValueType.Double;
        cSeries.Label = "#VALY";
        #endregion

        foreach (DataRow row in dataTable.Rows)
        {
            if (IsNullOrEmpty(row["abslow"].ToString()))
            {
                continue;
            }

            var rowInfo = new HistoryESRow(row);
            cSeries.Points.AddXY(rowInfo.Date, rowInfo.Abshigh - rowInfo.Abslow);
        }

        chart.Series.Add(cSeries);
    }

    public async Task DrawTargetZoneCandleChart(List<DataTableRowV1> rowList, DataTable dataTable, Chart chart, Label textField, CancellationToken cToken)
    {

        #region ChartSetup
        var chartSize = new CandleChartSizeInfo(dataTable);
        chart.ChartAreas[0].AxisY.Minimum = chartSize.YMin;
        chart.ChartAreas[0].AxisY.LabelStyle.Format = "0";
        chart.ChartAreas[0].AxisX.LabelStyle.Format = "MM-dd HH:mm";
        chart.Series.Clear();

        var cSeries = new Series();
        cSeries.ChartArea = "ChartArea1";
        cSeries.ChartType = SeriesChartType.Candlestick;
        cSeries.XValueType = ChartValueType.DateTime;
        cSeries.YValueType = ChartValueType.Double;
        cSeries.YValuesPerPoint = 4;

        var midLineSeries = new Series();
        midLineSeries.ChartArea = "ChartArea1";
        midLineSeries.ChartType = SeriesChartType.Point;
        midLineSeries.XValueType = ChartValueType.DateTime;
        midLineSeries.YValueType = ChartValueType.Double;
        #endregion

        // Get unique days from data
        var dayList = new List<int>();
        var dayAvgList = new List<decimal>();
        foreach (var row in rowList)
        {
            var thisDay = row.ChartTime.Value.Day;
            if (!dayList.Contains(thisDay))
            {
                dayList.Add(thisDay);
            }
        }

        // Split each day into am/pm and process datapoints
        for (var i = 0; i < dayList.Count; i++)
        {
            var day = dayList[i];

            // Sort the AM/PM entries for this day into lists
            var amDayDataList = rowList.FindAll(
                delegate (DataTableRowV1 row)
                {
                    return row.ChartTime.Value.Day == day && row.ChartTime.Value.Hour < 12;
                });
            var pmDayDataList = rowList.FindAll(
                delegate (DataTableRowV1 row)
                {
                    return row.ChartTime.Value.Day == day && row.ChartTime.Value.Hour > 12;
                });

            await TZChartBuilder(amDayDataList, i == dayList.Count - 1 ? true : false);
            await TZChartBuilder(pmDayDataList, i == dayList.Count - 1 ? true : false);
        }

        // get midValue average
        decimal dayAvgCalc = 0;
        var dayAvgCount = dayAvgList.Count;
        foreach (var value in dayAvgList)
        {
            dayAvgCalc += value;
        }
        dayAvgCalc = Math.Round(dayAvgCalc / dayAvgCount, 2);
        textField.BackColor = Color.Black;
        textField.ForeColor = Color.White;
        textField.Text = @$"Cross-day AM average values for the {dayAvgCount} days shown:{Environment.NewLine}Low:{(decimal).8 * dayAvgCalc}  |  Actual: {dayAvgCalc}  |  High: {(decimal)1.2 * dayAvgCalc}";

        //chart.DataBind();
        chart.Series.Add(cSeries);
        chart.Series.Add(midLineSeries);

        async Task TZChartBuilder(List<DataTableRowV1> list, bool lastDay)
        {
            // Calc the Midline for the entries
            decimal thisLow = -1;
            decimal thisHigh = -1;

            foreach (var entry in list)
            {
                if (thisLow == -1 || thisLow > entry.Low)
                {
                    thisLow = entry.Low;
                }
                if (thisHigh == -1 || thisHigh < entry.High)
                {
                    thisHigh = entry.High;
                }
            }
            var thisMidline = Math.Round((thisHigh - thisLow) / 2 + thisLow, 2);
            var dayAvg = Math.Round(thisHigh - thisLow, 2);

            dayAvgList.Add(dayAvg);

            // Graph things
            foreach (var candle in list)
            {
                object[] candleInfo = { candle.Low, candle.High, candle.Open, candle.Close };
                var thisC = cSeries.Points.AddXY(candle.ChartTime, candleInfo);
                var thisML = midLineSeries.Points.AddXY(candle.ChartTime, thisMidline);

                // Set dataPoint colors
                if (candle.Open > candle.Close)
                {
                    cSeries.Points[thisC].Color = Color.FromArgb(255, Color.OrangeRed);
                    cSeries.Points[thisC].BackSecondaryColor = Color.FromArgb(255, Color.OrangeRed);
                }
                else
                {
                    cSeries.Points[thisC].Color = Color.FromArgb(255, Color.ForestGreen);
                }
                midLineSeries.Points[thisML].Color = Color.FromArgb(255, Color.Black);

                // if the last entry for this day, add annotation to midline
                if (candle.ChartTime == list[list.Count - 1].ChartTime)
                    await AddAnnotation(chart, midLineSeries.Points[thisML], $"{thisMidline}", cToken);

                // If the last day in the day list add all the extra lines
                if (lastDay == true)
                {
                    var graphUpperHigh = (int)(thisHigh + dayAvg);
                    var graphUpperML = (int)(thisMidline + dayAvg);
                    var graphHigh = (int)thisHigh;
                    var graphLow = (int)thisLow;
                    var graphLowerML = (int)(thisMidline - dayAvg);
                    var graphLowerLow = (int)(thisLow - dayAvg);

                    var upperHighPoint = midLineSeries.Points.AddXY(candle.ChartTime, graphUpperHigh);
                    var upperMLPoint = midLineSeries.Points.AddXY(candle.ChartTime, graphUpperML);
                    var highPoint = midLineSeries.Points.AddXY(candle.ChartTime, graphHigh);
                    var lowPoint = midLineSeries.Points.AddXY(candle.ChartTime, graphLow);
                    var lowerMLPoint = midLineSeries.Points.AddXY(candle.ChartTime, graphLowerML);
                    var lowerLowPoint = midLineSeries.Points.AddXY(candle.ChartTime, graphLowerLow);

                    midLineSeries.Points[upperHighPoint].Color = Color.FromArgb(90, Color.ForestGreen);
                    midLineSeries.Points[upperMLPoint].Color = Color.FromArgb(90, Color.ForestGreen);
                    midLineSeries.Points[highPoint].Color = Color.FromArgb(90, Color.ForestGreen);
                    midLineSeries.Points[lowPoint].Color = Color.FromArgb(90, Color.OrangeRed);
                    midLineSeries.Points[lowerMLPoint].Color = Color.FromArgb(90, Color.OrangeRed);
                    midLineSeries.Points[lowerLowPoint].Color = Color.FromArgb(90, Color.OrangeRed);

                    // if the last entry for this day, add annotation to midline
                    if (candle.ChartTime == list[^1].ChartTime)
                    {
                        await AddAnnotation(chart, midLineSeries.Points[upperHighPoint], $"{graphUpperHigh}", cToken);
                        await AddAnnotation(chart, midLineSeries.Points[upperMLPoint], $"{graphUpperML}", cToken);
                        await AddAnnotation(chart, midLineSeries.Points[highPoint], $"{graphHigh}", cToken);
                        await AddAnnotation(chart, midLineSeries.Points[lowPoint], $"{graphLow}", cToken);
                        await AddAnnotation(chart, midLineSeries.Points[lowerMLPoint], $"{graphLowerML}", cToken);
                        await AddAnnotation(chart, midLineSeries.Points[lowerLowPoint], $"{graphLowerLow}", cToken);
                    }
                }
            }
        }
    }*//*

    private static string? GetTransactionTooltipText(DataTableRowV2 row)
    {
        if (row.TPrice is null)
            return null;

        var pList = Array.ConvertAll(row.TPrice.Split(","), decimal.Parse);
        var eList = row.TPosEffect!.Split(",").ToList();

        var toolTipText = $"Time: {row.ChartTime:HH:mm tt}{Environment.NewLine}--------------------{Environment.NewLine}";
        for (var i = 0; i < pList.Length; i++)
        {
            toolTipText += $"{pList[i]} -- {eList[i]}{Environment.NewLine}";
            if (i < pList.Length - 1)
                continue;

            return toolTipText;
        }

        return null;
    }

    private static Color GetTransactionPointColor(List<string> data, int i)
    {
        if (data.Count > 1)
            return Color.DarkMagenta;

        switch (data[i])
        {
            case "BuyToOpen":
                return Color.Red;
            case "SellToOpen":
                return Color.MediumBlue;
            case "BuyToClose":
            case "SellToClose":
                return Color.DarkOrange;
        }

        return Color.Black;
    }



    public async Task SetChartSize(List<TCandle> candleList)
    {
        var yMin = (double)candleList
            .Select(x => new[] { x.Open, x.Close, x.Low, x.High })
            .SelectMany(y => y)
            .Min();
        var yMax = (double)candleList
            .Select(x => new[] { x.Open, x.Close, x.Low, x.High })
            .SelectMany(y => y)
            .Max();
        var volMin = (double)candleList.Select(x => x.Volume).Min();
        var volMax = (double)candleList.Select(x => x.Volume).Max();

        var yIntervals = new List<double> { 10, 25, 50, 100, 250, 500 };
        var yIntervalApprox = (yMax - yMin) / 100;
        var yInterval = yIntervals.First(x => x > yIntervalApprox);

        chart1.ChartAreas.First().AxisY.Minimum = yMin - yInterval;
        chart1.ChartAreas.First().AxisY.Maximum = yMax + yInterval;
        chart1.ChartAreas.First().AxisY.LabelStyle.Format = "0";
        chart1.ChartAreas.First().AxisX.LabelStyle.Format = "HH:mm";

    }
}*/