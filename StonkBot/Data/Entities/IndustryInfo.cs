using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StonkBot.Data.Entities;

public class IndustryInfo
{
    [Key][Column(Order = 1)] public string Symbol { get; set; } = null!;
    
    public string? Sector { get; set; }
    public string? Industry { get; set; }
    public string? Category { get; set; }
}