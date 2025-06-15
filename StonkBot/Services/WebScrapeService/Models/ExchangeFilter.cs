namespace StonkBot.Services.WebScrapeService.Models;

public class ExchangeFilter : IRequestFilter
{
    public string? left { get; set; }
    public string? operation { get; set; }
    public string[]? right { get; set; }


    public ExchangeFilter(string[]? x)
    {
        left = "exchange";
        operation = "in_range";
        right = x;
    }
}