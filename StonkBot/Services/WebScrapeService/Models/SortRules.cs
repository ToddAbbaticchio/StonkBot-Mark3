namespace StonkBot.Services.WebScrapeService.Models;

public class SortRules
{
    public string sortBy { get; set; }
    public string sortOrder { get; set; }

    public SortRules(string x, string y)
    {
        sortBy = x;
        sortOrder = y;
    }
}
