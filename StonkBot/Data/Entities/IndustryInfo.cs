using System.ComponentModel.DataAnnotations;

namespace StonkBot.Data.Entities;

public class IndustryInfo
{
    [Key] public string Symbol { get; set; } = null!;
    public string? Sector { get; set; }
    public string? Industry { get; set; }
    public string? Category { get; set; }
}