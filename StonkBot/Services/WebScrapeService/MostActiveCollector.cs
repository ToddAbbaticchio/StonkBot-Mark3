using System.Text.Json;
using PuppeteerSharp;
using StonkBot.Data.Entities;
using StonkBot.Options;

namespace StonkBot.Services.WebScrapeService;

public partial interface IWebScraper
{
    Task<List<MostActive?>?> ScrapeMostActiveData(CancellationToken cToken);
}

internal partial class WebScraper
{
    public async Task<List<MostActive?>?> ScrapeMostActiveData(CancellationToken cToken)
    {
        const string jsCode = @"() => {
            let table = document.querySelector(""#js-category-content > div.js-base-screener-page-component-root > div > div.content-vcNgbmvT > div.root-cFX_j1gd > div.shadow-zuRb9wy5.screener-container-is-scrolled-to-end > div.tableWrap-SfGgNYTG > div > div > table"");
            let uBound = table.rows.length;
            let scrapedInfo = [];
            for (let i = 1; i < uBound; i++) {
                let symbol = table.rows[i].cells[0].children[0].children[2].text;
                let date = new Date().toISOString().split('T')[0];
                let order = i;
                
                let obj = {Symbol: symbol, Date: date, Order: order};
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
            await page.GoToAsync(Constants.MostActiveUrl);
            await page.WaitForNetworkIdleAsync();

            var result = (await page.EvaluateFunctionAsync(jsCode))!.Value.EnumerateArray().ToList();
            return result.Select(x => JsonSerializer.Deserialize<MostActive>(x.GetRawText())).ToList();
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