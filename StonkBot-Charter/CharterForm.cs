using StonkBot.Data.Enums;
using StonkBotChartoMatic.Charter;
using StonkBotChartoMatic.Charter.Extensions;
using StonkBotChartoMatic.Services.DbConnService;
using StonkBotChartoMatic.Services.FileUtilService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.Versioning;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Windows.Forms.DataVisualization.Charting;

namespace StonkBotChartoMatic;

[SupportedOSPlatform("windows10.0.17763.0")]
public sealed partial class CharterForm
{
    private readonly IDbConn _dbConn;
    private readonly IFileUtil _fileUtil;
    private readonly CancellationToken cToken = new CancellationTokenSource().Token;
    public DateTime selectedDate;
    public int selectedCandleLen = 1;
    public DataTable? dataTable;
    public string? droppedFilePath;

    public DroppedFileMode droppedFileMode;

    public CharterForm(IDbConn dbConn, IFileUtil fileUtil)
    {
        _dbConn = dbConn;
        _fileUtil = fileUtil;
        
        InitializeComponent();
        InitialChartConfig();
    }

    #region Event Handlers
    private void Chart_RightClick(object? sender, MouseEventArgs e)
    {
        if (e.Button != MouseButtons.Right)
            return;

        chart1.ChartAreas[0].AxisX.ScaleView.ZoomReset(1);
        chart1.ChartAreas[0].AxisY.ScaleView.ZoomReset(1);
        chart2.ChartAreas[0].AxisX.ScaleView.ZoomReset(1);
        chart2.ChartAreas[0].AxisY.ScaleView.ZoomReset(1);
    }

    private async void Form1_DragDrop(object? sender, DragEventArgs e)
    {
        try
        {
            // Handle FileDrop data.
            if (e.Data == null || !e.Data.GetDataPresent(DataFormats.FileDrop))
                return;

            var files = (string[])e.Data.GetData(DataFormats.FileDrop)!;
            if (files.Length > 1)
            {
                MessageBox.Show(@"One file at a time please!");
                return;
            }

            if (Path.GetExtension(files.First()) != ".csv")
            {
                MessageBox.Show($@"The provided file is a {Path.GetExtension(files.First())}, please provide a .csv");
                return;
            }

            droppedFilePath = files.First();
            var droppedFileName = Path.GetFileName(files.First());
            if (!droppedFileName.Contains("trade-station"))
            {
                MessageBox.Show(@"The provided file does not seem to be a valid TradeStation export!");
                return;
            }

            droppedFileMode = DroppedFileMode.TradeStation;
            var getDate = new Regex("(\\d{4}-\\d{2}-\\d{2})");
            var match = getDate.Match(droppedFileName);
            if (!match.Success)
                return;

            var dateString = match.Groups[1].Captures[0].Value;
            var fileDate = DateTime.Parse(dateString);
            DatePicker.Value = fileDate;
            var dialogResult = MessageBox.Show($@"Import data from: {droppedFilePath}?", @"Import data from file?", MessageBoxButtons.YesNo);
            if (dialogResult != DialogResult.Yes)
                return;

            var tCandles = await _fileUtil.Import(droppedFilePath, selectedDate, (SbCharterMarket)MarketDrop.SelectedValue!, cToken);
            await DrawCandleChartWithTransactionPoints(tCandles, selectedCandleLen, cToken);
        }
        catch (Exception ex)
        {
            MessageBox.Show(@"Error", ex.Message);
        }
    }

    private static void Form1_DragEnter(object? sender, DragEventArgs e)
    {
        if (e.Data == null)
            return;

        e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop) ? DragDropEffects.Copy : DragDropEffects.None;
    }
    #endregion

    #region Chart Zooming
    private const float CZoomScale = 1.25f;
    private int FZoomLevel = 0;

    private void Chart_MouseWheel(object? sender, MouseEventArgs e)
    {
        try
        {
            var xAxis = chart1.ChartAreas[0].AxisX;
            var xMin = xAxis.ScaleView.ViewMinimum;
            var xMax = xAxis.ScaleView.ViewMaximum;
            var xPixelPos = xAxis.PixelPositionToValue(e.Location.X);

            var yAxis = chart1.ChartAreas[0].AxisY;
            var yMin = yAxis.ScaleView.ViewMinimum;
            var yMax = yAxis.ScaleView.ViewMaximum;
            var yPixelPos = yAxis.PixelPositionToValue(e.Location.Y);

            var x2Axis = chart2.ChartAreas[0].AxisX;
            var y2Axis = chart2.ChartAreas[0].AxisY;

            if (e.Delta < 0 && FZoomLevel > 0)
            {
                // Scrolled down, meaning zoom out
                if (--FZoomLevel <= 0)
                {
                    FZoomLevel = 0;
                    xAxis.ScaleView.ZoomReset();
                    yAxis.ScaleView.ZoomReset();
                    x2Axis.ScaleView.ZoomReset();
                    y2Axis.ScaleView.ZoomReset();
                }
                else
                {
                    var xStartPos = Math.Max(xPixelPos - (xPixelPos - xMin) * CZoomScale, 0);
                    var xEndPos = Math.Min(xStartPos + (xMax - xMin) * CZoomScale, xAxis.Maximum);
                    xAxis.ScaleView.Zoom(xStartPos, xEndPos);
                    var yStartPos = Math.Max(yPixelPos - (yPixelPos - yMin) * CZoomScale, 0);
                    var yEndPos = Math.Min(yStartPos + (yMax - yMin) * CZoomScale, yAxis.Maximum);
                    yAxis.ScaleView.Zoom(yStartPos, yEndPos);

                    Chart_MatchPosition(xStartPos, xEndPos);
                }
            }
            else if (e.Delta > 0)
            {
                // Scrolled up, meaning zoom in
                var xStartPos = Math.Max(xPixelPos - (xPixelPos - xMin) / CZoomScale, 0);
                var xEndPos = Math.Min(xStartPos + (xMax - xMin) / CZoomScale, xAxis.Maximum);
                xAxis.ScaleView.Zoom(xStartPos, xEndPos);
                var yStartPos = Math.Max(yPixelPos - (yPixelPos - yMin) / CZoomScale, 0);
                var yEndPos = Math.Min(yStartPos + (yMax - yMin) / CZoomScale, yAxis.Maximum);
                yAxis.ScaleView.Zoom(yStartPos, yEndPos);
                FZoomLevel++;

                Chart_MatchPosition(xStartPos, xEndPos);
            }
        }
        catch
        {
        }
    }

    private void Chart_MatchPosition(double xStartPos, double xEndPos) => chart2.ChartAreas[0].AxisX.ScaleView.Zoom(xStartPos, xEndPos);

    #endregion

    #region Tool Interaction Functions
    private void ChartTypeDrop_SelectedIndexChanged(object sender, EventArgs e)
    {
        chart1.Annotations.Clear();
        chart1.Series.Clear();
        chart2.Series.Clear();
        var x = this.Width - 16;
        var y = this.Height - 83;
        var c1_y = Convert.ToInt32(y * .75);
        var c2_y = Convert.ToInt32(y * .25);

        switch (ChartTypeDrop.SelectedValue)
        {
            case SBChart.ESCandle:
                chart1.Size = new Size(x, c1_y);
                chart2.Size = new Size(x, c2_y);
                chart2.Location = new Point(0, c1_y - 7);

                chart2.Show();
                MarketDrop.Show();
                CandleDrop.Show();
                //ShowLabels.Show();
                TextField.Hide();
                break;
            case SBChart.FluxValue:
                chart1.Size = new Size(x, y);

                MarketDrop.Hide();
                CandleDrop.Hide();
                //ShowLabels.Hide();
                chart2.Hide();
                TextField.Hide();
                break;
            case SBChart.ESHighLow:
                chart1.Size = new Size(x, y);

                MarketDrop.Hide();
                CandleDrop.Hide();
                //ShowLabels.Hide();
                chart2.Hide();
                TextField.Hide();
                break;
            case SBChart.TargetZone:
                chart1.Size = new Size(x, y);
                TextField.Text = "";
                //TextField.Size = new Size(252, 26);
                TextField.Location = new Point(x / 2 - 126, 0);

                MarketDrop.Hide();
                CandleDrop.Show();
                //ShowLabels.Hide();
                chart2.Hide();
                TextField.Show();
                break;
        }
    }

    private void DatePicker_ValueChanged(object sender, EventArgs e)
    {
        selectedDate = DatePicker.Value;
        if (selectedDate.Date == DateTime.Now.Date)
        {
            MarketDrop.Text = @"Both";
        }
    }

    private void MarketDrop_SelectedIndexChanged(object sender, EventArgs e)
    {

        //if (MarketDrop.Text == "Both") selectedMarket = null;
        //else selectedMarket = MarketDrop.Text;
    }

    private void CandleDrop_SelectedIndexChanged(object sender, EventArgs e)
    {
        selectedCandleLen = Convert.ToInt32(CandleDrop.Text);
    }

    private async void UpdateButton_Click(object sender, EventArgs e)
    {
        List<DataTableRowV1> candleList;
        switch (ChartTypeDrop.SelectedValue)
        {
            // Handling for ES Candle Charts
            case SBChart.ESCandle:
                if (string.IsNullOrEmpty(droppedFilePath))
                {
                    dataTable = await _dbConn.esCandleQuery(selectedDate, (SbCharterMarket)MarketDrop.SelectedValue!, cToken);
                    candleList = CandleResizer.SetSize(dataTable, selectedCandleLen);
                    //await _chartUtil.DrawCandleChart(candleList, dataTable, chart1, chart2, cToken);
                    break;
                }

                //dataTable = await _fileUtil.Import(droppedFilePath, selectedDate, (SbCharterMarket)MarketDrop.SelectedValue!, cToken);
                //await _chartUtil.DrawCandleChartWithTransactionPoints(dataTable, chart1, chart2, selectedCandleLen, cToken);
                var tCandles = await _fileUtil.Import(droppedFilePath, selectedDate, (SbCharterMarket)MarketDrop.SelectedValue!, cToken);
                await DrawCandleChartWithTransactionPoints(tCandles, selectedCandleLen, cToken);
                return;

            // Handling for FluxValue Charts
            case SBChart.FluxValue:
                chart1.Annotations.Clear();
                dataTable = await _dbConn.historyESQuery(selectedDate, 10, cToken);
                //await _chartUtil.DrawFluxValueChart(dataTable, chart1, cToken);
                break;

            // Handling for ES High/Low Charts
            case SBChart.ESHighLow:
                chart1.Annotations.Clear();
                dataTable = await _dbConn.historyESQuery(selectedDate, 10, cToken);
                //await _chartUtil.DrawESHighLowChart(dataTable, chart1, cToken);
                break;

            // Handling for multi day zone chart
            case SBChart.TargetZone:
                chart1.Annotations.Clear();
                dataTable = await _dbConn.targetZoneQuery(selectedDate, 3, cToken);
                candleList = CandleResizer.SetSize(dataTable, selectedCandleLen);
                //await _chartUtil.DrawTargetZoneCandleChart(candleList, dataTable, chart1, TextField, cToken);
                break;
        }
    }

    #endregion

    private void Chart1_AxisViewChanged(object? sender, ViewEventArgs e)
    {
        if (e.Axis.AxisName.ToString() != "X")
        {
            return;
        }

        var viewMin = chart1.ChartAreas[0].AxisX.ScaleView.ViewMinimum;
        var viewMax = chart1.ChartAreas[0].AxisX.ScaleView.ViewMaximum;

        var min = new DataPoint(0, double.MaxValue);
        var max = new DataPoint(0, double.MinValue);

        var points = chart1.Series[0].Points;
        foreach (var point in points)
        {
            if (point.XValue >= viewMin && point.XValue <= viewMax)
            {
                if (point.YValues[0] > max.YValues[0])
                    max = point;

                if (point.YValues[0] < min.YValues[0])
                    min = point;
            }
            else if (point.XValue >= viewMax)
                break;
        }
        Chart_MatchPosition(min.XValue, max.XValue);
    }
}