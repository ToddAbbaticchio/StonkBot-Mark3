using System.Diagnostics;
using StonkBot.Services.ConsoleWriter.Enums;

namespace StonkBot.Services.SbActions;

public partial interface ISbAction
{
    Task EarningsReportCheck(CancellationToken cToken);
}

internal partial class SbAction
{
    public async Task EarningsReportCheck(CancellationToken cToken)
    {
        var timer = new Stopwatch();
        _con.WriteLog(MessageSeverity.Section, _targetLog, "SbActionRunner.ErScrape - Starting...");
        timer.Start();
        await ErScrape(cToken);
        timer.Stop();
        _con.WriteLog(MessageSeverity.Info, _targetLog, $"Elapsed time: [{timer.Elapsed}]");

        timer.Reset();

        _con.WriteLog(MessageSeverity.Section, _targetLog, "SbActionRunner.ErCheck - Starting...");
        timer.Start();
        await ErCheck(cToken);
        timer.Stop();
        _con.WriteLog(MessageSeverity.Info, _targetLog, $"Elapsed time: [{timer.Elapsed}]");
    }
}