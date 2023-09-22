using StonkBot.Options;
using StonkBot.Services.ConsoleWriter;

namespace StonkBot.Services.WebScrapeService;

internal partial class WebScraper : IWebScraper
{
    private readonly IConsoleWriter _con;
    private readonly HttpClient _httpClient;
    private readonly SbVars _vars;

    public WebScraper(IConsoleWriter con, HttpClient httpClient, SbVars vars)
    {
        _con = con;
        _httpClient = httpClient;
        _vars = vars;
    }
}