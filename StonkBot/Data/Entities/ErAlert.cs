namespace StonkBot.Data.Entities;

public class ErAlert
{
    public Guid Id { get; set; }   
    public string Symbol { get; set; } = null!;
    public DateTime Date { get; set; }
    public string Type { get; set; } = null!;
}