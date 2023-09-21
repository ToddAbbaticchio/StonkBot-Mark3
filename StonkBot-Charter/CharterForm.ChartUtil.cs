using StonkBot.Data.Enums;
using StonkBotChartoMatic.Charter.Extensions;
using StonkBotChartoMatic.Services.FileUtilService.Enums;
using StonkBotChartoMatic.Services.FileUtilService.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms.DataVisualization.Charting;

namespace StonkBotChartoMatic;

public sealed partial class CharterForm
{
    public async Task DrawCandleChartWithTransactionPoints(List<TCandle> tCandles, int candleSize, CancellationToken cToken)
    {
        chart1.Series.Clear();
        chart2.Series.Clear();
        await SetChartSize(tCandles);

        // Config and add series to charts
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
        //tSeries.IsXValueIndexed = true;
        chart1.Series.Add(tSeries);

        var vSeries = new Series();
        vSeries.Name = "VolumeSeries";
        vSeries.ChartArea = "Chart2Area";
        vSeries.ChartType = SeriesChartType.Column;
        vSeries.XValueType = ChartValueType.DateTime;
        vSeries.YValueType = ChartValueType.Double;
        chart2.Series.Add(vSeries);

        foreach (var candle in tCandles)
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

            // If we have a transaction, plot it
            foreach (var t in candle.Transactions)
            {
                var thisT = tSeries.Points.AddXY(candle.ChartTime, t!.Price);

                // group by price
                var priceGroup = candle.Transactions
                    .Where(x => x!.Price == t.Price)
                    .ToList();

                // Grouped handling
                if (priceGroup.Count > 1)
                {
                    tSeries.Points[thisT].Color = t.OrderType switch
                    {
                        OrderType.BuyToOpen => Color.Red,
                        OrderType.SellToOpen => Color.MediumBlue,
                        OrderType.BuyToClose => Color.DarkOrange,
                        OrderType.SellToClose => Color.DarkOrange,
                        _ => tSeries.Points[thisT].Color
                    };

                    if (priceGroup.Any(x => x!.OrderType != t.OrderType))
                        tSeries.Points[thisT].Color = Color.Fuchsia;
                    
                    var groupTooltip = $"Time: {candle.ChartTime:HH:mm tt}{Environment.NewLine}--------------------{Environment.NewLine}";
                    priceGroup.ForEach(x => groupTooltip += $"{x!.Price} -- {x.OrderType}{Environment.NewLine}");
                    tSeries.Points[thisT].ToolTip = groupTooltip;
                    continue;
                }

                // Solo handling
                tSeries.Points[thisT].Color = t.OrderType switch
                {
                    OrderType.BuyToOpen => Color.Red,
                    OrderType.SellToOpen => Color.MediumBlue,
                    OrderType.BuyToClose => Color.DarkOrange,
                    OrderType.SellToClose => Color.DarkOrange,
                    _ => tSeries.Points[thisT].Color
                };

                // Set tooltip
                tSeries.Points[thisT].ToolTip = $"Time: {candle.ChartTime:HH:mm tt}{Environment.NewLine}--------------------{Environment.NewLine}{t.Price} -- {t.OrderType}";
            }
        }
    }

    public Task SetChartSize(List<TCandle> candleList)
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


        // Chart1 Setup
        chart1.ChartAreas.First().Name = "Chart1Area";
        chart1.ChartAreas.First().AxisY.Minimum = yMin - yInterval;
        chart1.ChartAreas.First().AxisY.Maximum = yMax + yInterval;
        chart1.ChartAreas.First().AxisY.LabelStyle.Format = "0";
        chart1.ChartAreas.First().AxisX.LabelStyle.Format = "HH:mm";

        // Chart2 Setup
        chart2.ChartAreas.First().Name = "Chart2Area";
        chart2.ChartAreas.First().AxisY.LabelStyle.Format = "0";
        chart2.ChartAreas.First().AxisX.LabelStyle.Format = "HH:mm";
        return Task.CompletedTask;
    }

    public void InitialChartConfig()
    {
        // Form settings
        AllowDrop = true;
        DragDrop += Form1_DragDrop;
        DragEnter += Form1_DragEnter;
        chart1.MouseWheel += Chart_MouseWheel;
        //chart1.SelectionRangeChanged += Match_SelectionRangeChanged;
        chart1.MouseClick += Chart_RightClick;
        
        chart1.ChartAreas[0].CursorX.IsUserEnabled = true;
        chart1.ChartAreas[0].CursorY.IsUserEnabled = true;
        chart1.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
        chart1.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
        chart1.ChartAreas[0].CursorX.LineColor = Color.Transparent;
        chart1.ChartAreas[0].CursorY.LineColor = Color.Transparent;
        chart1.ChartAreas[0].CursorX.SelectionColor = Color.PaleTurquoise;
        chart1.ChartAreas[0].CursorY.SelectionColor = Color.PaleTurquoise;
        chart1.ChartAreas[0].CursorX.Interval = 0;
        chart1.ChartAreas[0].CursorY.Interval = 0;
        chart1.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.WhiteSmoke;
        chart1.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.WhiteSmoke;
        chart1.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
        chart1.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
        chart1.ChartAreas[0].AxisX.ScaleView.SmallScrollSizeType = DateTimeIntervalType.Minutes;
        chart1.ChartAreas[0].AxisX.ScaleView.SmallScrollSize = 30;
        chart1.ChartAreas[0].AxisX.ScaleView.SmallScrollMinSizeType = DateTimeIntervalType.Minutes;
        chart1.ChartAreas[0].AxisX.ScaleView.SmallScrollMinSize = 1;

        chart2.ChartAreas[0].CursorX.IsUserEnabled = true;
        chart2.ChartAreas[0].CursorY.IsUserEnabled = true;
        chart2.ChartAreas[0].CursorX.IsUserSelectionEnabled = true;
        chart2.ChartAreas[0].CursorY.IsUserSelectionEnabled = true;
        chart2.ChartAreas[0].CursorX.LineColor = Color.Transparent;
        chart2.ChartAreas[0].CursorY.LineColor = Color.Transparent;
        chart2.ChartAreas[0].CursorX.SelectionColor = Color.PaleTurquoise;
        chart2.ChartAreas[0].CursorY.SelectionColor = Color.PaleTurquoise;
        chart2.ChartAreas[0].CursorX.Interval = 0;
        chart2.ChartAreas[0].CursorY.Interval = 0;
        chart2.ChartAreas[0].AxisX.MajorGrid.LineColor = Color.WhiteSmoke;
        chart2.ChartAreas[0].AxisY.MajorGrid.LineColor = Color.WhiteSmoke;
        chart2.ChartAreas[0].AxisX.ScaleView.Zoomable = true;
        chart2.ChartAreas[0].AxisY.ScaleView.Zoomable = true;
        chart2.ChartAreas[0].AxisX.ScaleView.SmallScrollSizeType = DateTimeIntervalType.Minutes;
        chart2.ChartAreas[0].AxisX.ScaleView.SmallScrollSize = 30;
        chart2.ChartAreas[0].AxisX.ScaleView.SmallScrollMinSizeType = DateTimeIntervalType.Minutes;
        chart2.ChartAreas[0].AxisX.ScaleView.SmallScrollMinSize = 1;

        chart1.AxisViewChanged += Chart1_AxisViewChanged;
        ChartTypeDrop.DataSource = Enum.GetValues(typeof(SBChart));
        MarketDrop.DataSource = Enum.GetValues(typeof(SbCharterMarket));
        DatePicker.Value = DateTime.Now;
        CandleDrop.Text = "1";
        Text = @"StonkBot_ChartoMatic";
    }
}