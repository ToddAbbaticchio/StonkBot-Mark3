using System.ComponentModel.DataAnnotations;

namespace StonkBot.Data.Entities;
public class DiscordMessageRecord
{
    [Key] public ulong MessageId { get; set; }
    public string Channel { get; set; } = null!;
    public DateTime Date { get; set; }
}