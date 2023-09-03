using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using StonkBotChartoMatic.Services.FileUtilService.Enums;

namespace StonkBotChartoMatic.Services.FileUtilService.Models;

public class TCandle
{
    [Key] public DateTime ChartTime { get; set; }
    public decimal Open { get; set; }
    public decimal Close { get; set; }
    public decimal Low { get; set; }
    public decimal High { get; set; }
    public decimal Volume { get; set; }
    public List<TCandleTransaction?> Transactions { get; set; } = new();
}

public class TCandleTransaction
{
    public DateTime ExecTime { get; set; }
    public decimal Price { get; set; }
    public string Side { get; set; } = null!;
    public OrderType OrderType { get; set; }
}
