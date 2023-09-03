using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;

namespace StonkBotChartoMatic.Charter.Extensions;

public class CandleChartSizeInfo
{
    public double XInterval { get; set; }
    public double YMin { get; set; }
    public double YMax { get; set; }
    public double YInterval { get; set; }
    public double VolumeMax { get; set; }

    public CandleChartSizeInfo(DataTable dataTable)
    {
        if (dataTable.Rows.Count == 0)
        {
            MessageBox.Show(@"The database doesn't contain any entries that match the requested date and market time(s).");
            return;
        }
        try
        {
            // prep work
            var yVal = Convert.ToDouble(dataTable.Compute("min([Low])", ""));
            var VolMax = Convert.ToDouble(dataTable.Compute("max([Volume])", ""));
            var YIntervals = new List<double> { 10, 25, 50, 100, 250, 500 };
            var YIntervalApprox = (YMax - YMin) / 100;

            // apply values
            YMin = 25 * Math.Floor(yVal / 25);
            YMax = Convert.ToDouble(dataTable.Compute("max([High])", ""));
            VolumeMax = VolMax;
            YInterval = YIntervals.MinBy(x => Math.Abs(YIntervalApprox - x));
            XInterval = 15;
        }
        catch (Exception ex)
        {
            MessageBox.Show($@"Error creating CandleChartSizeInfo object: {ex.Message}");
        }
    }
}

public class FluxChartSizeInfo
{
    public double XInterval { get; set; }
    public double YMin { get; set; }
    public double YMax { get; set; }
    public double YInterval { get; set; }

    public FluxChartSizeInfo(DataTable dataTable)
    {
        if (dataTable.Rows.Count == 0)
        {
            MessageBox.Show(@"The database doesn't contain any entries that match the requested date and market time(s).");
            return;
        }
        try
        {
            // prep work
            var yMinMath = Convert.ToDouble(dataTable.Compute("min([Absflux])", "")) - 10;
            var yMin = yMinMath < 0 ? 0 : yMinMath;
            var YIntervals = new List<double> { 10, 25, 50, 100, 250, 500 };
            var YIntervalApprox = (YMax - YMin) / 100;

            // apply values

            YMin = yMin;
            YMax = Convert.ToDouble(dataTable.Compute("max([Absflux])", ""));
            YInterval = YIntervals.MinBy(x => Math.Abs(YIntervalApprox - x));
            XInterval = 1;

        }
        catch (Exception ex)
        {
            MessageBox.Show($@"Error creating FluxChartSizeInfo object: {ex.Message}");
        }
    }
}

public class ESHighLowSizeInfo
{
    public double XInterval { get; set; }
    public double YMin { get; set; }
    public double YMax { get; set; }
    public double YInterval { get; set; }

    public ESHighLowSizeInfo(DataTable dataTable)
    {
        if (dataTable.Rows.Count == 0)
        {
            MessageBox.Show(@"The database doesn't contain any entries that match the requested date and market time(s).");
            return;
        }
        try
        {
            // prep work
            var abslow = Convert.ToDouble(dataTable.Compute("min([Low])", ""));
            var abshigh = Convert.ToDouble(dataTable.Compute("min([High])", ""));
            var YIntervals = new List<double> { 10, 25, 50, 100, 250, 500 };
            var YIntervalApprox = (YMax - YMin) / 100;

            // apply values

            YMin = abslow;
            YMax = abshigh;
            YInterval = YIntervals.MinBy(x => Math.Abs(YIntervalApprox - x));
            XInterval = 1;

        }
        catch (Exception ex)
        {
            MessageBox.Show($@"Error creating FluxChartSizeInfo object: {ex.Message}");
        }
    }
}