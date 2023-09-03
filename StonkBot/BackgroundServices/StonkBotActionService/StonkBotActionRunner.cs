using StonkBot.Data.Enums;
using StonkBot.Services.BackupService;
using StonkBot.Services.ConsoleWriter;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.SbActions;
using StonkBot.Services.SbActions._Enums;
using StonkBot.Services.SbActions._Models;

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

    public StonkBotActionRunner(IConsoleWriter con, ISbAction sbAction, IDbBackupService dbBackup)
    {
        _con = con;
        _sbActions = sbAction;
        _dbBackup = dbBackup;
    }

    public async Task<ActionSchedule> Execute(ActionSchedule actionSchedule, CancellationToken cToken)
    {
        /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
        /*~~~~~  Dumb / Special / Manual / Testing Commands Here:  ~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
        /*~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~*/
        //await _dbBackup.SbBackupChecks(cToken);
        //await _sbActions.UpdateCalculatedFields(CalcField.All, cToken);
        //await _sbActions.ScanFixMissingPeriod(DateTime.Today.SbDate(), "year", 1, cToken);
        //await _sbAction.IpoCheck(cToken);
        //await _sbAction.GetMarketData(cToken);
        //await _sbAction.DiscordAlert(AlertType.FourHandAlert, cToken);
        //await _sbActions.IpoCheck(cToken);
        //await _sbActions.Csv2DbTable(cToken);
        //await _sbActions.CheckSecondPass(DateTime.Today.SbDate(), cToken);
        //await _sbActions.IpoCheck(cToken);
        //await _sbActions.SimulateCheckFirstPass("ENLT", DateTime.Parse("05/12/2023"), cToken);
        //await _sbActions.ErCheck(cToken);
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
            { "DbBackupChecks", () => _dbBackup.SbBackupChecks(cToken) }
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
            // Set next run time
            while (action.StartTime <= currentTime)
            {
                action.StartTime += action.Interval;
            }

            // Run the action
            try
            {
                await actionHandler[action.Method]();
                _con.WriteLog(MessageSeverity.Stats, $"SbActionRunner.{action.Method} complete! Next run queued for: {action.StartTime}");
            }
            catch (Exception ex)
            {
                _con.WriteLog(MessageSeverity.Error, $"Error running {action.Method}: {ex.Message}");
            }
        }
        return actionSchedule;
    }
}