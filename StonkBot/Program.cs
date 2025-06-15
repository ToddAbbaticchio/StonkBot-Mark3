using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StonkBot.Appsettings.Models;
using StonkBot.BackgroundServices.StonkBotActionService;
using StonkBot.BackgroundServices.StonkBotStreamingService;
using StonkBot.Data;
using StonkBot.MarketPatterns;
using StonkBot.Services.BackupService;
using StonkBot.Services.CharlesSchwab.APIClient;
using StonkBot.Services.ConnectionCheck;
using StonkBot.Services.ConsoleWriter;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.DiscordService;
using StonkBot.Services.WebScrapeService;
using System.Reflection;
using ISbAction = StonkBot.Services.SbActions.ISbAction;
using SbAction = StonkBot.Services.SbActions.SbAction;

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
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    var assembly = Assembly.GetExecutingAssembly();
                    using var stream = assembly.GetManifestResourceStream("appsettings.json");
                    if (stream == null)
                        throw new FileNotFoundException("Could not find appsettings.json embedded resource");
                    
                    using var reader = new StreamReader(stream);
                    var fileContents = reader.ReadToEnd();

                    config.AddJsonStream(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(fileContents)));
                })
                .ConfigureServices((hostContext, services) =>
                {
                    // Inject config data from appsettings.json
                    services.Configure<DbConfig>(config =>
                        hostContext.Configuration.GetSection("DbConfig").Bind(config));
                    services.Configure<DiscordConfig>(config =>
                        hostContext.Configuration.GetSection("DiscordConfig").Bind(config));
                    services.Configure<SchwabApiConfig>(config =>
                        hostContext.Configuration.GetSection("SchwabApiConfig").Bind(config));

                    // Shared services
                    services.AddDbContext<IStonkBotDb, StonkBotDbContext>();
                    services.AddSingleton<IConsoleWriter, ConsoleWriter>();
                    services.AddSingleton<IDiscordMessager, DiscordMessager>();
                    services.AddSingleton<IConnectionChecker, ConnectionChecker>();
                    services.AddSingleton<IDbBackupService, DbBackupService>();
                    //services.AddSingleton<ISchwabApiAuthenticator, SchwabApiAuthenticator>();

                    // StonkBotStreamService services
                    services.AddHostedService<StonkBotStreamService>();
                    services.AddScoped<IStonkBotStreamRunner, StonkBotStreamRunner>();
                    //services.AddSingleton<TdaStreamingClient>();

                    // StonkBotActionService Services
                    services.AddHostedService<StonkBotActionService>();
                    services.AddScoped<IStonkBotActionRunner, StonkBotActionRunner>();
                    services.AddTransient<ISbAction, SbAction>();
                    //services.AddTransient<ITdaApiClient, TdaApiClient>();
                    services.AddTransient<ISchwabApiClient, SchwabApiClient>();
                    services.AddTransient<IWebScraper, WebScraper>();
                    services.AddTransient<IMarketPatternMatcher, MarketPatternMatcher>();
                    services.AddTransient<HttpClient>();

                    // suppress startup logging
                    services.AddLogging(x => { 
                        x.AddFilter("Microsoft.Hosting.Lifetime", LogLevel.Warning);
                        x.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None);
                    });
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