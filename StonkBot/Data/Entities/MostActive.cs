using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace StonkBot.Data.Entities;

public class MostActive
{
    [Key][Column(Order = 1)] public string Symbol { get; set; } = null!;
    [Key][Column(Order = 2)] public DateTime Date { get; set; } = DateTime.Today;
    public int Order { get; set; }
}