using Microsoft.Extensions.Hosting;
using StonkBot.Services.ConnectionCheck;
using StonkBot.Services.ConsoleWriter;
using StonkBot.Services.ConsoleWriter.Enums;

namespace StonkBot.BackgroundServices.StonkBotStreamingService;

public class StonkBotStreamService : IHostedService, IDisposable
{
    private readonly CancellationTokenSource _cts;
    private readonly IConsoleWriter _con;
    private readonly IConnectionChecker _connCheck;
    private int _threadRunning;
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);
    private const string ServiceName = nameof(StonkBotStreamService);

    private readonly IStonkBotStreamRunner _streamRunner;

    public StonkBotStreamService(IConsoleWriter con, IConnectionChecker connCheck, IStonkBotStreamRunner streamRunner)
    {
        _cts = new CancellationTokenSource();
        _con = con;
        _connCheck = connCheck;
        _streamRunner = streamRunner;
    }

    public Task StartAsync(CancellationToken cToken)
    {
        _con.WriteLog(MessageSeverity.Section, TargetLog.StreamingData, $"{ServiceName} starting!");
        _ = Task.Run(InitializeRunnerLoopAsync, cToken);
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cToken)
    {
        _con.WriteLog(MessageSeverity.Section, TargetLog.StreamingData, $"{ServiceName} stopping!");
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
        while (!_cts.IsCancellationRequested)
        {
            try
            {
                if (Interlocked.CompareExchange(ref _threadRunning, 1, 0) != 0)
                {
                    _con.WriteLog(MessageSeverity.Info, TargetLog.StreamingData, $"Previous {ServiceName} thread work is still in process. Trying again in {_interval} seconds...");
                    continue;
                }

                // check network connection
                if (await _connCheck.DcTest())
                {
                    _con.ReplaceLastLog(MessageSeverity.Error, TargetLog.StreamingData, "Internet connection is down...");
                    continue;
                }

                await _streamRunner.Execute(_cts.Token);
            }
            catch (Exception ex)
            {
                _con.WriteLog(MessageSeverity.Error, ex.Message);
            }
            finally
            {
                await Task.Delay(_interval, _cts.Token);
                Interlocked.Exchange(ref _threadRunning, 0);
            }
        }
    }
}