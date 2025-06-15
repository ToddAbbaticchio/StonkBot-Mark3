using PuppeteerSharp;
using StonkBot.Options;
using StonkBot.Services.ConsoleWriter;

namespace StonkBot.Services.WebScrapeService;

internal partial class WebScraper : IWebScraper
{
    private readonly IConsoleWriter _con;
    private readonly HttpClient _httpClient;

    public WebScraper(IConsoleWriter con, HttpClient httpClient)
    {
        _con = con;
        _httpClient = httpClient;
    }
}