using Microsoft.Extensions.DependencyInjection;
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
    //private readonly TimeSpan _interval = TimeSpan.FromSeconds(30);
    
    // 15 seconds just for the dance party
    private readonly TimeSpan _interval = TimeSpan.FromSeconds(15);
    private int _danceParty = 1;

    private const string ServiceName = nameof(StonkBotStreamService);
    //private readonly IServiceScopeFactory _scopeFactory;
    private readonly IStonkBotStreamRunner _streamRunner;

    public StonkBotStreamService(IConsoleWriter con, IConnectionChecker connCheck, IStonkBotStreamRunner streamRunner/*, IServiceScopeFactory scopeFactory*/)
    {
        _cts = new CancellationTokenSource();
        _con = con;
        _connCheck = connCheck;
        _streamRunner = streamRunner;
        //_scopeFactory = scopeFactory;
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

                /*using var scope = _scopeFactory.CreateScope();
                var streamRunner = scope.ServiceProvider.GetRequiredService<IStonkBotStreamRunner>();
                await streamRunner.Execute(_cts.Token);*/

                //await _streamRunner.Execute(_cts.Token);
                
                _danceParty = _con.DanceParty(_danceParty);
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