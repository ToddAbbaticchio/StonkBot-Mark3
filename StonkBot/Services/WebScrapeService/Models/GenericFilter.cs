namespace StonkBot.Services.WebScrapeService.Models;

public interface IRequestFilter { }

public class GenericFilter : IRequestFilter
{
    public string? left { get; set; }
    public string? operation { get; set; }
    public string? right { get; set; }


    public GenericFilter(string? Left, string? Operation)
    {
        left = Left;
        operation = Operation;
    }

    public GenericFilter(string? Left, string? Operation, string? Right)
    {
        left = Left;
        operation = Operation;
        right = Right;
    }
}