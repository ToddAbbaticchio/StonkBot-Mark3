using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StonkBot.BackgroundServices.StonkBotActionService;
using StonkBot.BackgroundServices.StonkBotStreamingService;
using StonkBot.Data;
using StonkBot.MarketPatterns;
using StonkBot.Options;
using StonkBot.Services.BackupService;
using StonkBot.Services.ConnectionCheck;
using StonkBot.Services.ConsoleWriter;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.DiscordService;
using StonkBot.Services.SbActions;
using StonkBot.Services.TDAmeritrade.APIClient;
using StonkBot.Services.TDAmeritrade.StreamingClient;
using StonkBot.Services.WebScrapeService;

namespace StonkBot;

public static class Program
{
    public static async Task Main()
    {
        Console.CursorVisible = false;
        var con = new ConsoleWriter();
        var cts = new CancellationTokenSource();

        try
        {
            var host = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    // Shared services
                    services.AddDbContext<IStonkBotDb, StonkBotDbContext>(ServiceLifetime.Transient);
                    services.AddSingleton<IConsoleWriter, ConsoleWriter>();
                    services.AddSingleton<IDiscordMessager, DiscordMessager>();
                    services.AddSingleton<IConnectionChecker, ConnectionChecker>();
                    services.AddTransient<IDbBackupService, DbBackupService>();
                    
                    // StonkBotStreamSvc services
                    services.AddHostedService<StonkBotStreamService>();
                    services.AddTransient<IStonkBotStreamRunner, StonkBotStreamRunner>();
                    services.AddSingleton<TdaStreamingClient>();

                    // StonkBotActionSvc Services
                    services.AddHostedService<StonkBotActionService>();
                    services.AddTransient<IStonkBotActionRunner, StonkBotActionRunner>();
                    services.AddTransient<ISbAction, SbAction>();
                    services.AddTransient<ITdaApiClient, TdaApiClient>();
                    services.AddTransient<IWebScraper, WebScraper>();
                    services.AddTransient<IMarketPatternMatcher, MarketPatternMatcher>();
                    services.AddTransient<HttpClient>();

                    // suppress startup logging
                    services.AddLogging(x => { x.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning); });
                })
                .Build();

            await host.RunAsync(cts.Token);
        }
        catch (Exception ex)
        {
            con.WriteLog(MessageSeverity.Error, ex.Message);
        }
    }
}