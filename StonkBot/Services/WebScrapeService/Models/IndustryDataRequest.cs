namespace StonkBot.Services.WebScrapeService.Models;

public class IndustryDataRequest
{
    public List<string> columns { get; set; }
    public List<IRequestFilter> filter { get; set; }
    public bool ignore_unknown_fields { get; set; }
    public Option options { get; set; }
    public List<int> range { get; set; }
    public SortRules sort { get; set; }
    public List<string> markets { get; set; }

    public IndustryDataRequest(string industry)
    {
        columns = new List<string>
        {
            "name",
            "volume",
            "relative_volume_10d_calc",
            "earnings_per_share_diluted_ttm",
            "Volatility.W",
            "Volatility.M",
            "total_revenue_ttm",
            "net_income_ttm",
            "market_cap_basic"
        };
        filter = new List<IRequestFilter>
        {
            new GenericFilter("name", "nempty"),
            new GenericFilter("typespecs", "has_none_of", "preferred"),
            new GenericFilter("industry", "equal", industry),
            new ExchangeFilter(new[] { "AMEX", "NASDAQ", "NYSE" })
        };
        ignore_unknown_fields = false;
        options = new Option("en");
        range = new List<int> { 0, 100 };
        sort = new SortRules("name", "asc");
        markets = new List<string> { "america" };
    }
}