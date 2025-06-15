using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace StonkBot.Data.Entities;

public class CalculatedFields
{
    [Key][Column(Order = 1)] public string Symbol { get; set; } = null!;
    [Key][Column(Order = 2)] public DateTime Date { get; set; }

    public string? FromYesterday { get; set; }
    public bool? UpToday { get; set; }
    public string? VolumeAlert { get; set; }
    public string? VolumeAlert2 { get; set; }
    public string? FiveDayStable { get; set; }
    public bool? UpperShadow { get; set; }
    public string? AboveUpperShadow { get; set; }
    public string? FHTargetDay { get; set; }
    public string? LastFHTarget { get; set; }
}