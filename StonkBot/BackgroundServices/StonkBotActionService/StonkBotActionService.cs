using Microsoft.Extensions.Hosting;
using StonkBot.Services.ConnectionCheck;
using StonkBot.Services.ConsoleWriter;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.SbActions._Models;

namespace StonkBot.BackgroundServices.StonkBotActionService;

public class StonkBotActionService : IHostedService, IDisposable
{
    private readonly CancellationTokenSource _cts;
    private readonly IConsoleWriter _con;
    private readonly IConnectionChecker _connCheck;
    private int _threadRunning;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);
    private const string ServiceName = nameof(StonkBotActionService);
    private readonly IStonkBotActionRunner _actionRunner;

    public StonkBotActionService(IConsoleWriter con, IConnectionChecker connCheck, IStonkBotActionRunner actionRunner)
    {
        _cts = new CancellationTokenSource();
        _con = con;
        _connCheck = connCheck;
        _actionRunner = actionRunner;
    }

    public Task StartAsync(CancellationToken cToken)
    {
        _con.WriteLog(MessageSeverity.Section, $"{ServiceName} starting!");
        _ = Task.Run(InitializeRunnerLoopAsync, cToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cToken)
    {
        _con.WriteLog(MessageSeverity.Section, $"{ServiceName} stopping!");
        _cts.Cancel();
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _cts.Dispose();
        GC.SuppressFinalize(this);
    }

    private async void InitializeRunnerLoopAsync()
    {
        var actionSchedule = new ActionSchedule();

        while (!_cts.IsCancellationRequested)
        {
            try
            {
                if (actionSchedule.CreatedOn != DateTime.Today)
                {
                    actionSchedule = new ActionSchedule();
                    _con.WriteLog(MessageSeverity.Info, $"Generated ActionSchedule for {DateTime.Today}!");
                }

                if (Interlocked.CompareExchange(ref _threadRunning, 1, 0) != 0)
                {
                    _con.WriteLog(MessageSeverity.Info, $"Previous {ServiceName} thread work is still in process. Trying again in {_interval} seconds...");
                    continue;
                }

                // check network connection
                if (await _connCheck.DcTest())
                {
                    _con.ReplaceLastLog(MessageSeverity.Error, TargetLog.ActionRunner, "Internet connection is down...");
                    continue;
                }

                _con.UpdateStatus(actionSchedule);
                actionSchedule = await _actionRunner.Execute(actionSchedule, _cts.Token);
            }
            catch (Exception ex)
            {
                _con.WriteLog(MessageSeverity.Error, ex.Message);
            }
            finally
            {
                Interlocked.Exchange(ref _threadRunning, 0);
                await Task.Delay(_interval, _cts.Token);
            }
        }
    }
}