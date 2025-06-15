namespace StonkBot.Services.WebScrapeService.Models;

public class IndustryDataResponse
{
    public int totalCount { get; set; }
    public List<DataItem> Data { get; set; } = null!;
}

public class DataItem
{
    public string S { get; set; } = null!;
    public List<object> D { get; set; } = null!;
}