using HtmlAgilityPack;
using StonkBot.Data.Entities;
using StonkBot.Extensions;
using StonkBot.Services.ConsoleWriter.Enums;

namespace StonkBot.Services.WebScrapeService;

public partial interface IWebScraper
{
    Task<List<IpoListing>> ScrapeIpoData(CancellationToken cToken);
}

internal partial class WebScraper
{
    public async Task<List<IpoListing>> ScrapeIpoData(CancellationToken cToken)
    {
        try
        {
            // scrape page
            HttpResponseMessage response = await _httpClient.GetAsync(_vars.IpoScrapeUrl, cToken);
            response.EnsureSuccessStatusCode();
            var result = await response.Content.ReadAsStringAsync(cToken);

            // process html data
            var htmlDocument = new HtmlDocument();
            htmlDocument.LoadHtml(result);
            var docNodes = htmlDocument.DocumentNode.DescendantsAndSelf().ToList();
            var scrapedIpos = ProcessDocNodes(docNodes);

            return scrapedIpos;
        }
        catch (Exception ex)
        {
            _con.WriteLog(MessageSeverity.Error, $"Error scraping IPO data: {ex.Message}");
            return new List<IpoListing>();
        }
    }

    private static List<IpoListing> ProcessDocNodes(List<HtmlNode> htmlNodes)
    {
        var ipoList = new List<IpoListing>();
        var node = 0;
        foreach (var docNode in htmlNodes)
        {
            #pragma warning disable IDE0170
            if (docNode is { Name: "a", NextSibling: { Name: "p" } })
            #pragma warning restore IDE0170
            {
                var subList = htmlNodes.GetRange(node, 27);
                subList.RemoveAll(x => x.Name == "p");
                subList.RemoveAll(x => x.Name == "span");
                subList.RemoveAll(x => x.Name == "div");

                var currListing = new IpoListing
                {
                    Symbol = subList[2].InnerHtml,
                    Name = subList[1].InnerHtml,
                    OfferingPrice = subList[3].InnerHtml,
                    OfferAmmount = subList[4].InnerHtml,
                    OfferingEndDate = DateTime.Parse(subList[5].InnerHtml).SbDate(),
                    ExpectedListingDate = DateTime.Parse(subList[6].InnerHtml).SbDate(),
                    ScrapeDate = DateTime.Today.SbDate()
                };
                ipoList.Add(currListing);
            }
            node++;
        }

        return ipoList;
    }
}