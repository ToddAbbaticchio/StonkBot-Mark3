using StonkBot.Data.Enums;
using System.ComponentModel.DataAnnotations;

namespace StonkBot.Data.Entities;
public class DiscordMessageRecord
{
    [Key] public ulong MessageId { get; set; }
    public string Channel { get; set; } = null!;
    public AlertType Type { get; set; } = AlertType.Unknown;
    public DateTime DateTime { get; set; }
}