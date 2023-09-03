using StonkBotChartoMatic.Services.FileUtilService.Enums;
using System;

namespace StonkBotChartoMatic.Services.FileUtilService.Models;

public class ImportTransaction
{
    public string Symbol { get; set; } = null!;
    public string Side { get; set; } = null!;
    public string Type { get; set; } = null!;
    public int Qty { get; set; }
    public int FilledQty { get; set; }
    public decimal? LimitPrice { get; set; }
    public decimal? StopPrice { get; set; }
    public decimal? AvgFillPrice { get; set; }
    public string Status { get; set; } = null!;
    public DateTime OpenTime { get; set; }
    public DateTime CloseTime { get; set; }
    public string Duration { get; set; } = null!;
    public decimal? CommissionFee { get; set; }
    public DateTime ExpirationDate { get; set; }
    public string OrderId { get; set; } = null!;
    public OrderType? OrderType { get; set; }
    public string? MatchedOrder { get; set; }

    public static ImportTransaction SplitTransaction(ImportTransaction t, int originalQuantity)
    {
        return new ImportTransaction
        {
            Symbol = t.Symbol,
            Side = t.Side,
            Type = t.Type,
            Qty = 1,
            FilledQty = 1,
            LimitPrice = t.LimitPrice,
            StopPrice = t.StopPrice,
            AvgFillPrice = t.AvgFillPrice,
            Status = t.Status,
            OpenTime = t.OpenTime,
            CloseTime = t.CloseTime,
            Duration = t.Duration,
            CommissionFee = t.CommissionFee / originalQuantity,
            ExpirationDate = t.ExpirationDate,
            OrderId = t.OrderId,
            OrderType = t.OrderType
        };
    }
}