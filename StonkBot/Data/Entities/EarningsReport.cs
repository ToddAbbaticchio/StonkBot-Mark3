using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StonkBot.Data.Entities;

public class EarningsReport
{
    [Key][Column(Order = 1)] public string Symbol { get; set; } = null!;
    [Key][Column(Order = 2)] public DateTime Date { get; set; }
    public DateTime PeriodEnding { get; set; }
    public string Time { get; set; } = null!;
    public List<ErAlert> Alerts { get; set; } = new();
}