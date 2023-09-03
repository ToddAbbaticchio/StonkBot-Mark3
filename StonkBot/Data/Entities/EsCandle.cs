using System.ComponentModel.DataAnnotations;

namespace StonkBot.Data.Entities;

public class EsCandle
{
    [Key] public DateTime ChartTime { get; set; }
    public decimal Open { get; set; }
    public decimal Close { get; set; }
    public decimal Low { get; set; }
    public decimal High { get; set; }
    public decimal Volume { get; set; }
}