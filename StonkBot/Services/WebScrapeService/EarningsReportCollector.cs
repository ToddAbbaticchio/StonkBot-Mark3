using System.Text.Json;
using PuppeteerSharp;
using StonkBot.Data.Entities;
using StonkBot.Migrations;
using StonkBot.Options;

namespace StonkBot.Services.WebScrapeService;

public partial interface IWebScraper
{
    Task<List<EarningsReport?>?> ScrapeErData(CancellationToken cToken);
}

internal partial class WebScraper
{
    public async Task<List<EarningsReport?>?> ScrapeErData(CancellationToken cToken)
    {
        // paths to the page elements we care about
        const string loadMoreVisible = "#js-category-content > div > div > div.tv-load-more.tv-load-more--screener.js-screener-load-more";
        //const string loadMoreHidden = "#js-category-content > div > div > div.tv-load-more.tv-load-more--screener.js-screener-load-more i-hidden";
        const string changeToWeekView = "#js-screener-container > div.tv-screener-toolbar.tv-screener-toolbar--standalone.tv-screener-toolbar--markets_absolute > div.tv-screener-toolbar__period-picker > div > div > div:nth-child(4)";

        // real dumb javascript table parse code
        const string jsCode = @"() => {
            let table = document.querySelector(""#js-screener-container > div.tv-screener__content-pane > table"");
            let uBound = table.children[2].children.length;
            let scrapedInfo = [];
            for (let i = 1; i <= uBound; i++) {
                let symbol = table.rows[i].cells[0].children[0].children[2].children[0].text;
                let date = table.rows[i].cells[8].innerText;
                let periodEnding = table.rows[i].cells[9].innerText;
                let time = table.rows[i].cells[10].title;
                let obj = {Symbol: symbol, Date: date, PeriodEnding: periodEnding, Time: time};
                scrapedInfo.push(obj);
            }
            return scrapedInfo;
        }";

        // create browser launch options / selector options
        var browser = await Puppeteer.LaunchAsync(new LaunchOptions()
            {
                Args = new[] { "--no-sandbox" },
                Headless = true,
                ExecutablePath = Constants.ChromePath,
            }
        );
        
        try
        {
            var page = await browser.NewPageAsync();
            //await page.SetUserAgentAsync("Mozilla/5.0 (X11; Linux x86_64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/64.0.3282.39 Safari/537.36");
            await page.GoToAsync(Constants.ErScrapeUrl);
            await page.ClickAsync(changeToWeekView);
            await page.WaitForNetworkIdleAsync();

            async Task<bool> MoreListingsAvailable()
            {
                try
                {
                    var checkButton = await page.QuerySelectorAsync(loadMoreVisible);
                    return checkButton != null;
                }
                catch (Exception ex)
                {
                    if (ex.Message.Contains("not visible or not an HTMLElement"))
                        return false;

                    throw new Exception($"Error during ErScrape (determining if there are more ErReports to load): {ex.Message}");
                }
            }

            while (await MoreListingsAvailable())
            {
                await page.ClickAsync(loadMoreVisible);
                await page.WaitForNetworkIdleAsync();
            }

            var result = (await page.EvaluateFunctionAsync(jsCode))!.Value.EnumerateArray().ToList();
            return result.Select(x => JsonSerializer.Deserialize<EarningsReport>(x.GetRawText())).ToList();
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
        finally
        {
            if (browser != null)
                await browser.DisposeAsync();
        }
    }
}