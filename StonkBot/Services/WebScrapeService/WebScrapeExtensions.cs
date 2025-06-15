using PuppeteerSharp;

namespace StonkBot.Services.WebScrapeService;

public static class WebScrapeExtensions
{
    public static async Task EnsureAllLoaded(this IPage? x)
    {
        var buttons = await x!.QuerySelectorAllAsync("button");
        while (await buttons.CanLoadMore())
        {
            await x.ClickButtonAsync("Load More");
        }
    }

    private static async Task<bool> CanLoadMore(this IEnumerable<IElementHandle> x)
    {
        var exists = false;
        foreach (var button in x)
        {
            var isAttached = await button.EvaluateFunctionAsync<bool>("x => document.body.contains(x)", button);
            if (!isAttached)
                continue;

            var buttonTextProperty = await button.GetPropertyAsync("innerText");
            var buttonText = buttonTextProperty?.RemoteObject.Value?.ToString()?.Trim();
            if (buttonText != "Load More")
                continue;

            exists = true;
        }

        return exists;
    }

    public static async Task ClickButtonAsync(this IPage? x, string innerText)
    {
        var buttons = await x!.QuerySelectorAllAsync("button");
        foreach (var button in buttons)
        {
            var buttonTextProperty = await button.GetPropertyAsync("innerText");
            var buttonText = buttonTextProperty?.RemoteObject.Value?.ToString()?.Trim();
            if (!buttonText?.StartsWith(innerText) ?? true)
                continue;

            await button.ClickAsync();
            await x.WaitForNetworkIdleAsync();
        }
    }
}
