using System.Net.WebSockets;
using System.Text;
using System.Web;
using Newtonsoft.Json;
using StonkBot.Extensions;
using StonkBot.Services.CharlesSchwab._Models;
using StonkBot.Services.CharlesSchwab.APIClient;
using StonkBot.Services.CharlesSchwab.StreamingClient.Models;
using StonkBot.Services.ConsoleWriter;
using StonkBot.Services.ConsoleWriter.Enums;

namespace StonkBot.Services.CharlesSchwab.StreamingClient;

public class SchwabStreamingClient : IDisposable
{
    private readonly ISchwabApiClient _apiClient;
    private readonly IConsoleWriter _con;
    private ClientWebSocket? _socket;
    private UserPrincipals? _userPrincipals;
    private Account? _account;
    private readonly StreamJsonProcessor _parser;
    private readonly JsonSerializerSettings _settings = new() { NullValueHandling = NullValueHandling.Ignore };
    private int _counter;
    private bool _connected;
    private readonly SemaphoreSlim _slim = new(1);
    public bool IsConnected
    {
        get
        {
            return _connected;
        }
        private set
        {
            if (_connected == value)
                return;
            _connected = value;
            OnConnect(value);
        }
    }

    public event Action<Exception> OnException = delegate { };
    public event Action<bool> OnConnect = delegate { };
    public event Action<string> OnJsonSignal = delegate { };
    public event Action<HeartbeatSignal> OnHeartbeatSignal = delegate { };
    public event Action<ChartSignal> OnChartSignal = delegate { };
    public event Action<QuoteSignal> OnQuoteSignal = delegate { };
    public event Action<TimeSaleSignal> OnTimeSaleSignal = delegate { };
    public event Action<BookSignal> OnBookSignal = delegate { };

    public SchwabStreamingClient(ISchwabApiClient apiClient, IConsoleWriter con)
    {
        _apiClient = apiClient;
        _con = con;

        _parser = new StreamJsonProcessor();
        _parser.OnHeartbeatSignal += x => { OnHeartbeatSignal(x); };
        _parser.OnChartSignal += x => { OnChartSignal(x); };
        _parser.OnQuoteSignal += x => { OnQuoteSignal(x); };
        _parser.OnTimeSaleSignal += x => { OnTimeSaleSignal(x); };
        _parser.OnBookSignal += x => { OnBookSignal(x); };
    }

    public async Task Connect(CancellationToken cToken)
    {
        try
        {
            if (_socket != null && _socket.State != WebSocketState.Closed)
            {
                throw new Exception("Busy");
            }

            _socket = new ClientWebSocket();
            _userPrincipals = await _apiClient.GetUserPrincipals(cToken);
            _account = _userPrincipals!.accounts.FirstOrDefault(x => x.accountId == _userPrincipals.primaryAccountId);
            var path = new Uri("wss://" + _userPrincipals.streamerInfo.streamerSocketUrl + "/ws");

            await _socket!.ConnectAsync(path, cToken);
            if (_socket.State == WebSocketState.Open)
            {
                await Login();
                Receive();
                IsConnected = true;
            }
        }
        catch (Exception ex)
        {
            OnException(ex);
            Cleanup();
        }
    }

    public async Task Disconnect(CancellationToken cToken)
    {
        if (_socket != null)
        {
            if (_socket.State == WebSocketState.Open)
            {
                await LogOut();
                await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "NormalClosure", CancellationToken.None);
                OnConnect(IsConnected);
            }

            _socket?.Dispose();
            _socket = null;
        }
        
        IsConnected = false;
    }

    public void Dispose()
    {
        Cleanup();
        GC.SuppressFinalize(this);
    }

    public Task SubscribeChart(string symbols, ChartSubs service)
    {
        if (_account == null || _userPrincipals == null)
        {
            var methodName = string.IsNullOrEmpty(System.Reflection.MethodBase.GetCurrentMethod()?.Name)
                ? System.Reflection.MethodBase.GetCurrentMethod()?.Name
                : "GetNameFailed";
            _con.WriteLog(MessageSeverity.Error, TargetLog.StreamingData, $"{methodName}: Missing _account and/or _userPrincipals objects");
            return Task.FromException(new Exception($"{methodName}: Missing required data"));
        }
        
        var request = new RealtimeRequestContainer
        {
            requests = new RealtimeRequest[]
            {
                new RealtimeRequest
                {
                    service = service.ToString(),
                    command = "SUBS",
                    requestid = Interlocked.Increment(ref _counter),
                    account = _account.accountId,
                    source = _userPrincipals.streamerInfo.appId,
                    parameters = new
                    {
                        keys = symbols,
                        fields = "0,1,2,3,4,5,6,7,8"
                    }
                }
            }
        };
        var data = JsonConvert.SerializeObject(request, _settings);
        return SendToServer(data);
    }

    public Task UnsubscribeChart(string symbols, ChartSubs service)
    {
        if (_account == null || _userPrincipals == null)
        {
            var methodName = string.IsNullOrEmpty(System.Reflection.MethodBase.GetCurrentMethod()?.Name)
                ? System.Reflection.MethodBase.GetCurrentMethod()?.Name
                : "GetNameFailed";
            _con.WriteLog(MessageSeverity.Error, TargetLog.StreamingData, $"{methodName}: Missing _account and/or _userPrincipals objects");
            return Task.FromException(new Exception($"{methodName}: Missing required data"));
        }

        var request = new RealtimeRequestContainer
        {
            requests = new RealtimeRequest[]
                {
                new RealtimeRequest
                {
                    service = service.ToString(),
                    command = "UNSUBS",
                    requestid = Interlocked.Increment(ref _counter),
                    account = _account.accountId,
                    source = _userPrincipals.streamerInfo.appId,
                    parameters = new
                    {
                        keys = symbols,
                    }
                }
                }
        };
        var data = JsonConvert.SerializeObject(request, _settings);
        return SendToServer(data);
    }

    public Task SubscribeQuote(string symbols)
    {
        if (_account == null || _userPrincipals == null)
        {
            var methodName = string.IsNullOrEmpty(System.Reflection.MethodBase.GetCurrentMethod()?.Name)
                ? System.Reflection.MethodBase.GetCurrentMethod()?.Name
                : "GetNameFailed";
            _con.WriteLog(MessageSeverity.Error, TargetLog.StreamingData, $"{methodName}: Missing _account and/or _userPrincipals objects");
            return Task.FromException(new Exception($"{methodName}: Missing required data"));
        }

        var request = new RealtimeRequestContainer
        {
            requests = new RealtimeRequest[]
            {
                new RealtimeRequest
                {
                    service = "QUOTE",
                    command = "SUBS",
                    requestid = Interlocked.Increment(ref _counter),
                    account = _account.accountId,
                    source = _userPrincipals.streamerInfo.appId,
                    parameters = new
                    {
                        keys = symbols,
                        fields = "0,1,2,3,4,5,8,9,10,11,12,13,14,15,24,28"
                    }
                }
            }
        };

        var data = JsonConvert.SerializeObject(request, _settings);
        return SendToServer(data);
    }

    public Task UnsubscribeQuote(string symbols)
    {
        if (_account == null || _userPrincipals == null)
        {
            var methodName = string.IsNullOrEmpty(System.Reflection.MethodBase.GetCurrentMethod()?.Name)
                ? System.Reflection.MethodBase.GetCurrentMethod()?.Name
                : "GetNameFailed";
            _con.WriteLog(MessageSeverity.Error, TargetLog.StreamingData, $"{methodName}: Missing _account and/or _userPrincipals objects");
            return Task.FromException(new Exception($"{methodName}: Missing required data"));
        }

        var request = new RealtimeRequestContainer
        {
            requests = new RealtimeRequest[]
            {
                new RealtimeRequest
                {
                    service = "QUOTE",
                    command = "UNSUBS",
                    requestid = Interlocked.Increment(ref _counter),
                    account = _account.accountId,
                    source = _userPrincipals.streamerInfo.appId,
                    parameters = new
                    {
                        keys = symbols,
                    }
                }
            }
        };

        var data = JsonConvert.SerializeObject(request, _settings);
        return SendToServer(data);
    }

    public Task SubscribeTimeSale(string symbols, TimeSaleServices service)
    {
        if (_account == null || _userPrincipals == null)
        {
            var methodName = string.IsNullOrEmpty(System.Reflection.MethodBase.GetCurrentMethod()?.Name)
                ? System.Reflection.MethodBase.GetCurrentMethod()?.Name
                : "GetNameFailed";
            _con.WriteLog(MessageSeverity.Error, TargetLog.StreamingData, $"{methodName}: Missing _account and/or _userPrincipals objects");
            return Task.FromException(new Exception($"{methodName}: Missing required data"));
        }

        var request = new RealtimeRequestContainer
        {
            requests = new RealtimeRequest[]
            {
                new RealtimeRequest
                {
                    service = service.ToString(),
                    command = "SUBS",
                    requestid = Interlocked.Increment(ref _counter),
                    account = _account.accountId,
                    source = _userPrincipals.streamerInfo.appId,
                    parameters = new
                    {
                        keys = symbols,
                        fields = "0,1,2,3,4"
                    }
                }
            }
        };

        var data = JsonConvert.SerializeObject(request, _settings);
        return SendToServer(data);
    }

    public Task UnsubscribeTimeSale(string symbols, TimeSaleServices service)
    {
        if (_account == null || _userPrincipals == null)
        {
            var methodName = string.IsNullOrEmpty(System.Reflection.MethodBase.GetCurrentMethod()?.Name)
                ? System.Reflection.MethodBase.GetCurrentMethod()?.Name
                : "GetNameFailed";
            _con.WriteLog(MessageSeverity.Error, TargetLog.StreamingData, $"{methodName}: Missing _account and/or _userPrincipals objects");
            return Task.FromException(new Exception($"{methodName}: Missing required data"));
        }

        var request = new RealtimeRequestContainer
        {
            requests = new RealtimeRequest[]
            {
                new RealtimeRequest
                {
                    service = service.ToString(),
                    command = "UNSUBS",
                    requestid = Interlocked.Increment(ref _counter),
                    account = _account.accountId,
                    source = _userPrincipals.streamerInfo.appId,
                    parameters = new
                    {
                        keys = symbols,
                    }
                }
            }
        };

        var data = JsonConvert.SerializeObject(request, _settings);
        return SendToServer(data);
    }

    public Task SubscribeBook(string symbols, BookOptions option)
    {
        if (_account == null || _userPrincipals == null)
        {
            var methodName = string.IsNullOrEmpty(System.Reflection.MethodBase.GetCurrentMethod()?.Name)
                ? System.Reflection.MethodBase.GetCurrentMethod()?.Name
                : "GetNameFailed";
            _con.WriteLog(MessageSeverity.Error, TargetLog.StreamingData, $"{methodName}: Missing _account and/or _userPrincipals objects");
            return Task.FromException(new Exception($"{methodName}: Missing required data"));
        }

        var request = new RealtimeRequestContainer
        {
            requests = new RealtimeRequest[]
            {
                new RealtimeRequest
                {
                    service = option.ToString(),
                    command = "SUBS",
                    requestid = Interlocked.Increment(ref _counter),
                    account = _account.accountId,
                    source = _userPrincipals.streamerInfo.appId,
                    parameters = new
                    {
                        keys = symbols,
                        fields = "0,1,2,3"
                    }
                }
            }
        };

        var data = JsonConvert.SerializeObject(request, _settings);
        return SendToServer(data);
    }

    public Task UnsubscribeBook(string symbols, BookOptions option)
    {
        if (_account == null || _userPrincipals == null)
        {
            var methodName = string.IsNullOrEmpty(System.Reflection.MethodBase.GetCurrentMethod()?.Name)
                ? System.Reflection.MethodBase.GetCurrentMethod()?.Name
                : "GetNameFailed";
            _con.WriteLog(MessageSeverity.Error, TargetLog.StreamingData, $"{methodName}: Missing _account and/or _userPrincipals objects");
            return Task.FromException(new Exception($"{methodName}: Missing required data"));
        }

        var request = new RealtimeRequestContainer
        {
            requests = new RealtimeRequest[]
            {
                new RealtimeRequest
                {
                    service = option.ToString(),
                    command = "UNSUBS",
                    requestid = Interlocked.Increment(ref _counter),
                    account = _account.accountId,
                    source = _userPrincipals.streamerInfo.appId,
                    parameters = new
                    {
                        keys = symbols,
                    }
                }
            }
        };

        var data = JsonConvert.SerializeObject(request, _settings);
        return SendToServer(data);
    }

    public Task RequestQOS(QOSLevels quality)
    {
        if (_account == null || _userPrincipals == null)
        {
            var methodName = string.IsNullOrEmpty(System.Reflection.MethodBase.GetCurrentMethod()?.Name)
                ? System.Reflection.MethodBase.GetCurrentMethod()?.Name
                : "GetNameFailed";
            _con.WriteLog(MessageSeverity.Error, TargetLog.StreamingData, $"{methodName}: Missing _account and/or _userPrincipals objects");
            return Task.FromException(new Exception($"{methodName}: Missing required data"));
        }

        var request = new RealtimeRequestContainer
        {
            requests = new RealtimeRequest[]
            {
                new RealtimeRequest
                {
                    service = "ADMIN",
                    command = "QOS",
                    requestid = Interlocked.Increment(ref _counter),
                    account = _account.accountId,
                    source = _userPrincipals.streamerInfo.appId,
                    parameters = new
                    {
                        qoslevel = ((int)quality)
                    }
                }
            }
        };

        var data = JsonConvert.SerializeObject(request, _settings);
        return SendToServer(data);
    }

    public async Task SendToServer(string data)
    {
        await _slim.WaitAsync();
        try
        {
            if (_socket != null)
            {
                var encoded = Encoding.UTF8.GetBytes(data);
                var buffer = new ArraySegment<byte>(encoded, 0, encoded.Length);
                await _socket.SendAsync(buffer, WebSocketMessageType.Text, true, CancellationToken.None);
            }
        }
        catch (Exception ex)
        {
            OnException(ex);
            Cleanup();
        }
        finally
        {
            _slim.Release();
        }
    }

    private async void Receive()
    {
        var buffer = new ArraySegment<byte>(new byte[2048]);
        try
        {
            do
            {
                WebSocketReceiveResult result;
                using var ms = new MemoryStream();
                do
                {
                    result = await _socket!.ReceiveAsync(buffer, CancellationToken.None);
                    ms.Write(buffer.Array!, buffer.Offset, result.Count);
                } while (!result.EndOfMessage);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    throw new Exception("WebSocketMessageType.Close");
                }

                ms.Seek(0, SeekOrigin.Begin);

                using var reader = new StreamReader(ms, Encoding.UTF8);
                var msg = await reader.ReadToEndAsync();
                HandleMessage(msg);
            } while (_socket is { State: WebSocketState.Open });
        }
        catch (Exception ex)
        {
            OnException(ex);
            Cleanup();
        }
    }

    private Task Login()
    {
        if (_account == null || _userPrincipals == null)
        {
            var methodName = string.IsNullOrEmpty(System.Reflection.MethodBase.GetCurrentMethod()?.Name)
                ? System.Reflection.MethodBase.GetCurrentMethod()?.Name
                : "GetNameFailed";
            _con.WriteLog(MessageSeverity.Error, TargetLog.StreamingData, $"{methodName}: Missing _account and/or _userPrincipals objects");
            return Task.FromException(new Exception($"{methodName}: Missing required data"));
        }

        //Converts ISO-8601 response in snapshot to ms since epoch accepted by Streamer
        var tokenTimeStampAsDateObj = _userPrincipals.streamerInfo.tokenTimestamp;
        var tokenTimeStampAsMs = tokenTimeStampAsDateObj.ToUnixTimeStamp();

        var queryString = HttpUtility.ParseQueryString(string.Empty);
        queryString.Add("userid", _account.accountId);
        queryString.Add("company", _account.company);
        queryString.Add("segment", _account.segment);
        queryString.Add("cddomain", _account.accountCdDomainId);
        queryString.Add("token", _userPrincipals.streamerInfo.token);
        queryString.Add("usergroup", _userPrincipals.streamerInfo.userGroup);
        queryString.Add("accessLevel", _userPrincipals.streamerInfo.accessLevel);
        queryString.Add("appId", _userPrincipals.streamerInfo.appId);
        queryString.Add("acl", _userPrincipals.streamerInfo.acl);
        queryString.Add("timestamp", tokenTimeStampAsMs.ToString());
        queryString.Add("authorized", "Y");

        var credits = queryString.ToString();
        var encoded = HttpUtility.UrlEncode(credits);

        var request = new RealtimeRequestContainer
        {
            requests = new RealtimeRequest[]
            {
                new RealtimeRequest
                {
                    service = "ADMIN",
                    command = "LOGIN",
                    requestid = Interlocked.Increment(ref _counter),
                    account = _account.accountId,
                    source = _userPrincipals.streamerInfo.appId,
                    parameters = new
                    {
                        token = _userPrincipals.streamerInfo.token,
                        version = "1.0",
                        credential = encoded,
                    }
                }
            }
        };
        var data = JsonConvert.SerializeObject(request);
        return SendToServer(data);
    }

    private Task LogOut()
    {
        if (_account == null || _userPrincipals == null)
        {
            var methodName = string.IsNullOrEmpty(System.Reflection.MethodBase.GetCurrentMethod()?.Name)
                ? System.Reflection.MethodBase.GetCurrentMethod()?.Name
                : "GetNameFailed";
            _con.WriteLog(MessageSeverity.Error, TargetLog.StreamingData, $"{methodName}: Missing _account and/or _userPrincipals objects");
            return Task.FromException(new Exception($"{methodName}: Missing required data"));
        }

        var request = new RealtimeRequestContainer
        {
            requests = new RealtimeRequest[]
            {
                new RealtimeRequest
                {
                    service = "ADMIN",
                    command = "LOGOUT",
                    requestid = Interlocked.Increment(ref _counter),
                    account = _account.accountId,
                    source = _userPrincipals.streamerInfo.appId,
                    parameters = new { }
                }
            }
        };
        var data = JsonConvert.SerializeObject(request);
        return SendToServer(data);
    }

    private void HandleMessage(string msg)
    {
        try
        {
            OnJsonSignal(msg);
            _parser.Parse(msg);
        }
        catch (Exception ex)
        {
            OnException(ex);
            //Do not cleanup, this is a user code issue
        }
    }

    private async void Cleanup()
    {
        if (_socket == null)
        {
            IsConnected = false;
            return;
        }
        
        if (_socket.State == WebSocketState.Open)
        {
            await LogOut();
            await _socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "NormalClosure", CancellationToken.None);
        }
        _socket.Dispose();
        _socket = null;
        IsConnected = false;
    }
}