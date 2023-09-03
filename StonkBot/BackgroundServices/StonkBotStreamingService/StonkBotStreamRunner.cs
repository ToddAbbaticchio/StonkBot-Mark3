using StonkBot.Data;
using StonkBot.Data.Entities;
using StonkBot.Extensions;
using StonkBot.Services.ConnectionCheck;
using StonkBot.Services.ConsoleWriter;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.TDAmeritrade.StreamingClient;
using StonkBot.Services.TDAmeritrade.StreamingClient.Models;

namespace StonkBot.BackgroundServices.StonkBotStreamingService;

public interface IStonkBotStreamRunner
{
    Task Execute(CancellationToken cToken);
}

public class StonkBotStreamRunner : IStonkBotStreamRunner
{
    // ReSharper disable once FieldCanBeMadeReadOnly.Local
    private readonly IStonkBotDb _db;
    private readonly IConsoleWriter _con;
    private readonly IConnectionChecker _connCheck;
    private readonly TdaStreamingClient _sClient;
    private readonly TargetLog _logWindow;
    private int _lastDance;

    public StonkBotStreamRunner(
        IStonkBotDb db,
        IConsoleWriter con,
        IConnectionChecker connCheck,
        TdaStreamingClient sClient)
    {
        _db = db;
        _con = con;
        _connCheck = connCheck;
        _logWindow = TargetLog.StreamingData;
        _sClient = sClient;
        _lastDance = 1;
    }

    public async Task Execute(CancellationToken cToken)
    {
        if (_sClient.IsConnected)
            return;

        /*_sClient.OnHeartbeatSignal += x =>
        {
            //_con.WriteLog(MessageSeverity.Info, _logWindow, $"Heartbeat SIGNAL!");
        };*/

        _sClient.OnChartSignal += async x =>
        {
            try
            {
                switch (x.symbol)
                {
                    case "/NQ":
                    {
                        var nqDupe = await _db.NqCandles.FindAsync(x.charttime.SbDateTime(), cToken);
                        if (nqDupe != null)
                            return;

                        await _db.NqCandles.AddAsync(new NqCandle
                        {
                            ChartTime = x.charttime.SbDateTime(),
                            Open = x.openprice,
                            Close = x.closeprice,
                            Low = x.lowprice,
                            High = x.highprice,
                            Volume = x.volume,
                        }, cToken);
                        break;
                    }
                    
                    case "/ES":
                    {
                        var esDupe = await _db.EsCandles.FindAsync(x.charttime.SbDateTime(), cToken);
                        if (esDupe != null)
                            return;

                        await _db.EsCandles.AddAsync( new EsCandle
                        {
                            ChartTime = x.charttime.SbDateTime(),
                            Open = x.openprice,
                            Close = x.closeprice,
                            Low = x.lowprice,
                            High = x.highprice,
                            Volume = x.volume,
                        }, cToken);
                        break;
                    }

                    default:
                    {
                        _con.WriteLog(MessageSeverity.Warning, TargetLog.StreamingData, $"Unexpected Symbol: {x.symbol}");
                        break;
                    }
                }
                
                await _db.SbSaveChangesAsync(cToken);
            }
            catch (Exception ex)
            {
                _con.WriteLog(MessageSeverity.Error, _logWindow, $"Error saving EsCandle for {x.charttime.SbDateTime()} - {ex.Message}");
            }
        };

        _sClient.OnJsonSignal += x =>
        {
            _lastDance = _con.DanceParty(_lastDance);
        };

        _sClient.OnConnect += async x =>
        {
            if (!x) await ConnectSocket(cToken);
        };

        await ConnectSocket(cToken);
    }
    
    private async Task ConnectSocket(CancellationToken cToken)
    {
        try
        {
            var disconnected = await _connCheck.DcTest();
            if (disconnected)
            {
                _con.ReplaceLastLog(MessageSeverity.Error, TargetLog.StreamingData, "Internet connection is down...");
                return;
            }

            _con.WriteLog(MessageSeverity.Info, _logWindow, "Socket not open. Connecting...");
            await _sClient.Connect(cToken);

            if (_sClient.IsConnected)
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cToken);
                await _sClient.SubscribeChart("/ES,/NQ", TDChartSubs.CHART_FUTURES);
                //await _sClient.SubscribeChart("/NQ", TDChartSubs.CHART_FUTURES);
            }
            else if (!cToken.IsCancellationRequested)
            {
                //await RetryConnect(cToken);
            }
        }
        catch (Exception ex)
        {
            _con.WriteLog(MessageSeverity.Error, _logWindow, ex.Message);
            //await RetryConnect(cToken);
        }
    }

    /*private async Task RetryConnect(CancellationToken cToken)
    {
        if (cToken.IsCancellationRequested)
            return;

        if (await _connCheck.DcTest())
        {
            _con.ReplaceLastLog(MessageSeverity.Error, TargetLog.StreamingData, "Internet connection is down...");
            return;
        }
            
        _con.WriteLog(MessageSeverity.Info, _logWindow, "Retrying...");
        await Task.Delay(TimeSpan.FromSeconds(5), cToken);
        await ConnectSocket(cToken);
    }*/
}