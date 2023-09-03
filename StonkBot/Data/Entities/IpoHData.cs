using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StonkBot.Data.Entities;

public class IpoHData
{
    [Key][Column(Order = 1)] public string Symbol { get; set; } = null!;
    [Key][Column(Order = 2)] public DateTime Date { get; set; }
    public decimal Open { get; set; }
    public decimal High { get; set; }
    public decimal Low { get; set; }
    public decimal Close { get; set; }
    public decimal Volume { get; set; }
}