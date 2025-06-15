using Newtonsoft.Json;
using PuppeteerSharp;
using StonkBot.Data.Entities;
using StonkBot.Options;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.WebScrapeService.Extensions;
using StonkBot.Services.WebScrapeService.Models;
using System.Collections.Concurrent;

namespace StonkBot.Services.WebScrapeService;

public partial interface IWebScraper
{
    Task<List<(IndustryInfo, IndustryInfoHData)>> ScrapeIndustryInfoData(CancellationToken cToken);
}

internal partial class WebScraper
{
    public async Task<List<(IndustryInfo, IndustryInfoHData)>> ScrapeIndustryInfoData(CancellationToken cToken)
    { 
        const string entryUrl = "https://www.tradingview.com/markets/stocks-usa/sectorandindustry-industry/";
        const string tradingViewApiUrl = "https://scanner.tradingview.com/america/scan";

        try
        {
            await using var browser = await Puppeteer.LaunchAsync
            (
                new LaunchOptions()
                {
                    Args = new[] { "--no-sandbox" },
                    Headless = false,
                    ExecutablePath = Constants.ChromePath,
                }
            );
            await using var page = await browser.NewPageAsync();
            await page.GoToAsync(entryUrl);
            await page.WaitForNetworkIdleAsync();
            await page.EnsureAllLoaded();

            // Process table
            var table = await page.QuerySelectorAllAsync("table");
            var rows = await table[1].QuerySelectorAllAsync("tbody tr");
            var rowDict = new Dictionary<string, string>();
            foreach (var row in rows)
            {
                // Pull Industry/Sector values from table
                var industry = await row.EvaluateFunctionAsync<string>("(x) => x.querySelector('td:first-child').innerText");
                var sector = await row.EvaluateFunctionAsync<string>("(x) => x.querySelector('td:nth-child(6)').innerText");
                if (sector.ToLower() == "miscellaneous")
                    continue;

                rowDict.Add(industry, sector);
            }

            var concurrentBag = new ConcurrentBag<(IndustryInfo, IndustryInfoHData)>();
            var options = new ParallelOptions { MaxDegreeOfParallelism = 10 };
            Parallel.ForEach(rowDict.Keys, options, industry =>
            {
                try
                {
                    var request = new IndustryDataRequest(industry);
                    var requestBody = new StringContent(JsonConvert.SerializeObject(request), System.Text.Encoding.UTF8, "application/json");
                    var response = _httpClient.PostAsync(tradingViewApiUrl, requestBody, cToken).Result;
                    var content = response.Content.ReadAsStringAsync(cToken).Result;
                    var responseData = JsonConvert.DeserializeObject<IndustryDataResponse>(content);
                    var iInfoList = responseData!.Data
                        .Select(x => x.ToIndustryInfo(rowDict[industry], industry))
                        .ToList();

                    if (iInfoList.Count != 0)
                        iInfoList.ForEach(x => concurrentBag.Add(x));
                }
                catch (Exception ex)
                {
                    _con.WriteLog(MessageSeverity.Error, TargetLog.ActionRunner, $"Failed to retrieve IndustryData for {industry}! {ex.Message}");
                }
            });

            return concurrentBag.ToList();
        }
        catch (Exception ex)
        {
            _con.WriteLog(MessageSeverity.Error, TargetLog.ActionRunner, $"Error during ScrapeIndustryInfoData: {ex.Message}");
            throw;
        }
    }
}