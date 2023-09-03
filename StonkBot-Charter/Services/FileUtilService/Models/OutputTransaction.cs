using System;
using System.Collections.Generic;
using System.Linq;

namespace StonkBotChartoMatic.Services.FileUtilService.Models;

internal class OutputTransaction
{
    public DateTime Date { get; set; }
    public string Day { get; set; }
    public string Symbol { get; set; }
    public string LongOrShort { get; set; }
    public string StartTime { get; set; }
    public string EndTime { get; set; }
    public string? TimeFrame { get; set; }
    public decimal? EnterPrice { get; set; }
    public decimal? ClosePrice { get; set; }
    public int OrderCount { get; set; }
    public int CloseCount { get; set; }
    public decimal? Profit { get; set; }
    public TimeSpan HoldTime { get; set; }
    public string OrderType { get; set; }

    public string ToOpenId { get; set; }
    public string ToCloseId { get; set; }

    public OutputTransaction(ImportTransaction tOpen, ImportTransaction tClose)
    {
        var profitMod = 1;
        if (tOpen.Symbol.StartsWith("ES"))
            profitMod = 50;
        if (tOpen.Symbol.StartsWith("MES"))
            profitMod = 5;
        if (tOpen.Symbol.StartsWith("NQ"))
            profitMod = 20;
        if (tOpen.Symbol.StartsWith("MNQ"))
            profitMod = 2;

        Date = tOpen.OpenTime;
        Day = tOpen.OpenTime.DayOfWeek.ToString();
        Symbol = tOpen.Symbol;
        LongOrShort = tOpen.OrderType == Enums.OrderType.BuyToOpen ? "Long" : "Short";
        StartTime = tOpen.OpenTime.ToString("HH:mm");
        EndTime = tClose.CloseTime.ToString("HH:mm");
        EnterPrice = new List<decimal?> { tOpen.LimitPrice, tOpen.AvgFillPrice }.Max();
        ClosePrice = new List<decimal?> { tClose.LimitPrice, tClose.StopPrice, tClose.AvgFillPrice }.Max();
        OrderCount = tOpen.Qty;
        CloseCount = tClose.Qty;
        Profit = LongOrShort == "Long" ? (ClosePrice - EnterPrice) * profitMod - (tOpen.CommissionFee + tClose.CommissionFee) : (EnterPrice - ClosePrice) * profitMod - (tOpen.CommissionFee + tClose.CommissionFee);
        HoldTime = tClose.CloseTime - tOpen.OpenTime;
        OrderType = tOpen.Type;
        ToOpenId = tOpen.OrderId;
        ToCloseId = tClose.OrderId;

        var h = tOpen.OpenTime.TimeOfDay;
        if (h >= TimeSpan.FromMinutes(1080) || h < TimeSpan.FromMinutes(120)) TimeFrame = "Asia";
        if (h >= TimeSpan.FromMinutes(120) && h < TimeSpan.FromMinutes(360)) TimeFrame = "Europe";
        if (h >= TimeSpan.FromMinutes(360) && h < TimeSpan.FromMinutes(570)) TimeFrame = "Early";
        if (h >= TimeSpan.FromMinutes(570) && h < TimeSpan.FromMinutes(630)) TimeFrame = "Open";
        if (h >= TimeSpan.FromMinutes(630) && h < TimeSpan.FromMinutes(900)) TimeFrame = "Mid";
        if (h >= TimeSpan.FromMinutes(900) && h < TimeSpan.FromMinutes(960)) TimeFrame = "Close";
        if (h >= TimeSpan.FromMinutes(960) && h < TimeSpan.FromMinutes(1020)) TimeFrame = "After";
    }

    public string GetOutputString(OutputTransaction o)
    {
        return $"{o.Symbol},{o.Date},{o.Day},{o.LongOrShort},{o.StartTime},{o.EndTime},{o.TimeFrame},{o.EnterPrice},{o.ClosePrice},{o.OrderCount},{o.CloseCount},{o.Profit},{o.HoldTime},{o.OrderType},,,,{o.ToOpenId},{o.ToCloseId}";
    }
}