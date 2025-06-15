using Microsoft.Extensions.Options;
using StonkBot.Appsettings.Models;
using StonkBot.Data.Enums;
using StonkBot.Extensions;
using StonkBot.Services.BackupService;
using StonkBot.Services.CharlesSchwab.APIClient;
using StonkBot.Services.ConsoleWriter;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.SbActions._Enums;
using StonkBot.Services.SbActions._Models;
using ISbAction = StonkBot.Services.SbActions.ISbAction;

namespace StonkBot.BackgroundServices.StonkBotActionService;

public interface IStonkBotActionRunner
{
    Task<ActionSchedule> Execute(ActionSchedule actionSchedule, CancellationToken cToken);
}

public class StonkBotActionRunner : IStonkBotActionRunner
{
    private readonly IConsoleWriter _con;
    private readonly ISbAction _sbActions;
    private readonly IDbBackupService _dbBackup;
    private readonly DbConfig _dbConfig;
    private readonly DiscordConfig _discordConfig;
    private readonly ISchwabApiClient _apiClient;
    
    public StonkBotActionRunner(IConsoleWriter con, ISbAction sbAction, IDbBackupService dbBackup, IOptions<DbConfig> dbConfig, IOptions<DiscordConfig> discordConfig, ISchwabApiClient apiClient)
    {
        _con = con;
        _sbActions = sbAction;
        _dbBackup = dbBackup;
        _dbConfig = dbConfig.Value;
        _discordConfig = discordConfig.Value;
        _apiClient = apiClient;
    }

    public async Task<ActionSchedule> Execute(ActionSchedule actionSchedule, CancellationToken cToken)
    {
        /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
        /*~~~~~  Dumb / Special / Manual / Testing Commands Here:  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
        /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
        //await _sbActions.IpoScrape(cToken);
        //await _sbActions.IndustryInfoScrape(cToken);
        //await _sbActions.IpoCheck(cToken);
        //await _sbActions.EarningsReportCheck(cToken);
        //await _sbActions.GetMarketData(cToken);
        //await _sbActions.UpdateCalculatedFields(CalcField.All, cToken);
        //await _sbActions.DiscordAlert(AlertType.FourHand, cToken);
        //await _dbBackup.SbBackupChecks(cToken);

        //var test1 = await _apiClient.GetQuoteAsync("AAPL", cToken);
        //var test2 = await _apiClient.GetCandlesAsync("ES", new DateTime(2025, 3, 14), cToken);

        //await _sbActions.ScrapeMostActive(cToken);

        //await _sbActions.ScanFixMissingPeriod(DateTime.Today.SbDate(), "year", 1, cToken);
        //await _sbActions.Csv2DbTable(cToken);
        //await _sbActions.CheckSecondPass(DateTime.Today.SbDate(), cToken);
        //await _sbActions.SimulateCheckFirstPass("ENLT", DateTime.Parse("05/12/2023"), cToken);
        //await _sbActions.IpoTableHealthCheck(cToken);
        /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/

        // action dictionary (bus driver)
        var actionHandler = new Dictionary<string, Func<Task>>
        {
            { "IpoScrape", () => _sbActions.IpoScrape(cToken) },
            { "IpoCheck", () => _sbActions.IpoCheck(cToken) },
            { "GetMarketData", () => _sbActions.GetMarketData(cToken) },
            { "UpdateCalculatedFields", () => _sbActions.UpdateCalculatedFields(CalcField.All, cToken) },
            { "DiscordAlert", () => _sbActions.DiscordAlert(AlertType.AllDailyAlerts, cToken) },
            { "EarningsReportCheck", () => _sbActions.EarningsReportCheck(cToken) },
            { "DbBackupChecks", () => _dbBackup.SbBackupChecks(cToken) },
            { "IndustryInfoScrape", () => _sbActions.IndustryInfoScrape(cToken) },
            { "DailyEsCandles", () => _sbActions.GetEsCandles(DateTime.Now.SbDate(), cToken) },
            { "DailyMostActive", () => _sbActions.ScrapeMostActive(cToken) }
        };

        // If actionSchedule isn't from today, make a new one
        if (DateTime.Today != actionSchedule.CreatedOn)
        {
            actionSchedule = new ActionSchedule();
            _con.WriteLog(MessageSeverity.Info, "Generated new ActionSchedule!");
        }
        
        // Check the action schedule.  Do the things.
        var currentTime = DateTime.Now;
        var eligibleActions = actionSchedule.ActionInfos
            .Where(x => actionHandler.ContainsKey(x.Method))
            .Where(x => currentTime > x.StartTime)
            .Where(x => currentTime < x.EndTime)
            .ToList();

        foreach (var action in eligibleActions)
        {
            try
            {
                action.SetNextRuntime();
                await actionHandler[action.Method]();
                _con.WriteLog(MessageSeverity.Stats, $"SbActionRunner.{action.Method} complete! Next run queued for: {action.StartTime}");
            }
            catch (Exception ex)
            {
                _con.WriteLog(MessageSeverity.Error, $"Error running {action.Method}: {(ex.InnerException != null ? ex.InnerException.Message : ex.Message)}");
            }
        }
        return actionSchedule;
    }
}