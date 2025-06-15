using System.Net;
using Microsoft.EntityFrameworkCore;
using RestSharp;
using StonkBot.Data;
using StonkBot.Services.CharlesSchwab.APIClient.Models;
using StonkBot.Services.ConsoleWriter;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.CharlesSchwab._Models;
using StonkBot.Appsettings.Models;
using Microsoft.Extensions.Options;
using System.Text;
using StonkBot.Extensions;
using static System.Runtime.InteropServices.JavaScript.JSType;
using Newtonsoft.Json.Linq;
using PuppeteerSharp;

namespace StonkBot.Services.CharlesSchwab.APIClient;

public interface ISchwabApiClient
{
    Task<string?> GetTokenAsync(CancellationToken cToken, bool force=false);
    Task<Quote?> GetQuoteAsync(string symbol, CancellationToken cToken);
    Task<List<Quote>?> GetQuotesAsync(List<string> symbols, CancellationToken cToken);
    Task<HistoricalDataResponse?> GetHistoricalDataAsync(string symbol, string periodType, string period, string frequencyType, string frequency, CancellationToken cToken);
    Task<UserPrincipals?> GetUserPrincipals(CancellationToken cToken);
    Task<List<Candle?>> GetCandlesAsync(string symbol, DateTime? targetDate, DateTime? startTime, DateTime? endTime, CancellationToken cToken);
}

public class SchwabApiClient : ISchwabApiClient
{
    private readonly IConsoleWriter _con;
    private readonly IStonkBotDb _db;
    private readonly TargetLog _logWindow;
    private readonly SchwabApiConfig _config;


    public SchwabApiClient(IConsoleWriter con, IStonkBotDb db, IOptions<SchwabApiConfig> schwabConfig)
    {
        _con = con;
        _db = db;
        _config = schwabConfig.Value;
        _logWindow = TargetLog.ActionRunner;
    }

    public async Task<string?> GetTokenAsync(CancellationToken cToken, bool force=false)
    {
        try
        {
            var dbToken = await _db.AuthTokens.FirstOrDefaultAsync(cToken) ?? throw new Exception("No auth token found!");
            if (!force && !dbToken.IsStale())
                return dbToken.access_token;

            var clientCredentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_config.ClientId}:{_config.AppSecret}"));
            using var client = new RestClient(_config.TokenUrl);
            var request = new RestRequest { Method = Method.Post };
            request.AddHeader("Authorization", $"Basic {clientCredentials}");
            request.AddParameter("grant_type", "refresh_token");
            request.AddParameter("refresh_token", dbToken.refresh_token);
            var response = await client.ExecuteAsync<AccessTokenRefreshResponse>(request);

            dbToken.tokenCreatedTime = response.Data!.tokenCreatedTime;
            dbToken.access_token = response.Data.access_token;
            dbToken.expires_in = response.Data.expires_in;
            dbToken.scope = response.Data.scope!;
            await _db.SbSaveChangesAsync(cToken);
            return response.Data.access_token;
        }
        catch (Exception ex)
        {
            _con.WriteLog(MessageSeverity.Error, $"SchwabApiClient.GetTokenAsync: {(ex.InnerException != null ? ex.InnerException.Message : ex.Message)}");
            throw new Exception(ex.Message);
        }
    }

    public async Task<Quote?> GetQuoteAsync(string symbol, CancellationToken cToken)
    {
        for (var i = 1; i <= 3; i++)
        {
            try
            {
                var token = await GetTokenAsync(cToken);
                using var client = new RestClient(_config.BaseUrl);
                var request = new RestRequest("quotes", Method.Get);
                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddQueryParameter("symbols", symbol);
                var response = await client.ExecuteAsync<QuoteWrapper>(request, cToken);
                                       
                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        break;
                    case HttpStatusCode.Unauthorized:
                        token = await GetTokenAsync(cToken, force: true);
                        await Task.Delay(TimeSpan.FromSeconds(3), cToken);
                        throw new Exception($"Bad response! [{response.StatusCode}] {response.StatusDescription}");
                    default:
                        throw new Exception($"Bad response for {symbol} quote! [{response.StatusCode}] {response.StatusDescription}");
                }

                return (Quote?)(response.Data!.Result ?? throw new Exception($"Failed to acquire Data.Result for {symbol}"));
            }
            catch (Exception ex)
            {
                _con.WriteLog(MessageSeverity.Warning, $"SchwabApiClient.GetQuoteAsync - {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(3), cToken);
            }
        }
        throw new Exception("Unhandled exception at end of GetQuoteAsync! Investigate this!");
    }

    public async Task<List<Quote>?> GetQuotesAsync(List<string> symbols, CancellationToken cToken)
    {
        for (var i = 1; i <= 3; i++)
        {
            try
            {
                var token = await GetTokenAsync(cToken);
                using var client = new RestClient(_config.BaseUrl);
                var request = new RestRequest($"quotes", Method.Get);
                request.AddHeader("Authorization", $"Bearer {token}");
                foreach (var symbol in symbols)
                {
                    request.AddQueryParameter("symbols", symbol);
                }
                var response = await client.ExecuteAsync<QuoteWrapper>(request, cToken);

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        break;
                    case HttpStatusCode.Unauthorized:
                        token = await GetTokenAsync(cToken, force: true);
                        await Task.Delay(TimeSpan.FromSeconds(3), cToken);
                        throw new Exception($"Bad response! [{response.StatusCode}] {response.StatusDescription}");
                    default:
                        throw new Exception($"Bad response for {symbols} quotes! [{response.StatusCode}] {response.StatusDescription}");
                }

                var result = response?.Data?.Result as List<Quote>;
                return result?.Where(x => x.symbol != null).ToList() ?? throw new Exception($"Failed to acquire Data.Result for {symbols}");
            }
            catch (Exception ex)
            {
                _con.WriteLog(MessageSeverity.Warning, $"Attempt {i} of 3 - Error: {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(3), cToken);
            }
        }
        throw new Exception("Unhandled exception at end of GetQuotesAsync! Investigate this!");
    }

    public async Task<HistoricalDataResponse?> GetHistoricalDataAsync(string symbol, string periodType, string period, string frequencyType, string frequency, CancellationToken cToken)
    {
        for (var i = 1; i <= 3; i++)
        {
            try
            {
                var token = await GetTokenAsync(cToken);
                using var client = new RestClient(_config.BaseUrl);
                var request = new RestRequest($"pricehistory", Method.Get);
                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddQueryParameter("symbol", symbol);
                request.AddQueryParameter("periodType", periodType);
                request.AddQueryParameter("period", period);
                request.AddQueryParameter("frequencyType", frequencyType);
                request.AddQueryParameter("frequency", frequency);
                
                var response = await client.ExecuteAsync<HistoricalDataResponse>(request, cToken);

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        break;
                    case HttpStatusCode.Unauthorized:
                        token = await GetTokenAsync(cToken, force: true);
                        await Task.Delay(TimeSpan.FromSeconds(3), cToken);
                        throw new Exception($"Bad response! [{response.StatusCode}] {response.StatusDescription}");
                    default:
                        throw new Exception($"Bad response for {symbol} price history! [{response.StatusCode}] {response.StatusDescription}");
                }

                return response.Data ?? throw new Exception($"Failed to acquire price history for {symbol}");
            }
            catch (Exception ex)
            {
                _con.WriteLog(MessageSeverity.Warning, $"Attempt {i} of 3 - Error: {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(3), cToken);
            }
        }
        throw new Exception($"Unhandled exception at end of GetHistoricalDataAsync for {symbol}.");
    }

    public async Task<List<Candle?>> GetCandlesAsync(string symbol, DateTime? targetDate, DateTime? startTime, DateTime? endTime, CancellationToken cToken)
    {
        for (var i = 1; i <= 3; i++)
        {
            try
            {
                string? start = null;
                string? end = null;
                
                if (targetDate != null)
                {
                    start = targetDate.Value.ToUnixTimeMillisecondsStartOfDayString();
                    end = targetDate.Value.ToUnixTimeMillisecondsEndOfDayString();
                }

                if (startTime != null && endTime != null)
                {
                    start = startTime.Value.ToUnixTimeMillisecondsString();
                    end = endTime.Value.ToUnixTimeMillisecondsString();
                }
                
                if (string.IsNullOrEmpty(start) || string.IsNullOrEmpty(end))
                    throw new Exception("Invalid date range supplied!");

                var token = await GetTokenAsync(cToken);
                using var client = new RestClient(_config.BaseUrl);
                var request = new RestRequest("pricehistory", Method.Get);
                request.AddHeader("Authorization", $"Bearer {token}");
                request.AddQueryParameter("symbol", symbol);
                request.AddQueryParameter("periodType", "day");
                request.AddQueryParameter("period", "1");
                request.AddQueryParameter("frequencyType", "minute");
                request.AddQueryParameter("frequency", "1");
                request.AddQueryParameter("startDate", start);
                request.AddQueryParameter("endDate", end);

                var response = await client.ExecuteAsync<HistoricalDataResponse>(request, cToken);

                switch (response.StatusCode)
                {
                    case HttpStatusCode.OK:
                        break;
                    case HttpStatusCode.Unauthorized:
                        token = await GetTokenAsync(cToken, force: true);
                        await Task.Delay(TimeSpan.FromSeconds(3), cToken);
                        throw new Exception($"Bad response! [{response.StatusCode}] {response.StatusDescription}");
                    default:
                        throw new Exception($"Bad response for {symbol} price history! [{response.StatusCode}] {response.StatusDescription}");
                }

                return response.Data?.candles?.Cast<Candle?>().ToList() ?? throw new Exception($"Failed to acquire candles for {symbol}");
            }
            catch (Exception ex)
            {
                _con.WriteLog(MessageSeverity.Warning, $"Attempt {i} of 3 - Error: {ex.Message}");
                await Task.Delay(TimeSpan.FromSeconds(3), cToken);
            }
        }
        throw new Exception($"Unhandled exception at end of GetCandlesAsync for {symbol}.");
    }

    public async Task<UserPrincipals?> GetUserPrincipals(CancellationToken cToken)
    {
        {
            for (var i = 1; i <= 3; i++)
            {
                try
                {
                    var token = await GetTokenAsync(cToken);
                    using var client = new RestClient(_config.BaseUrl);
                    var request = new RestRequest($"userprincipals", Method.Get);
                    request.AddHeader("Authorization", $"Bearer {token}");
                    request.AddQueryParameter("fields", "streamerSubscriptionKeys,streamerConnectionInfo");

                    var response = await client.ExecuteAsync<UserPrincipals>(request, cToken);

                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                            break;
                        case HttpStatusCode.Unauthorized:
                            token = await GetTokenAsync(cToken, force: true);
                            await Task.Delay(TimeSpan.FromSeconds(3), cToken);
                            throw new Exception($"Bad response! [{response.StatusCode}] {response.StatusDescription}");
                        default:
                            throw new Exception($"Bad response for user principals! [{response.StatusCode}] {response.StatusDescription}");
                    }

                    return response.Data ?? throw new Exception($"Failed to acquire user principals.");
                }
                catch (Exception ex)
                {
                    _con.WriteLog(MessageSeverity.Warning, $"Attempt {i} of 3 - Error: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(3), cToken);
                }
            }
            throw new Exception($"Unhandled exception at end of GetUserPrincipals.");
        }



        //for (var i = 1; i <= 3; i++)
        //{
        //    try
        //    {
        //        var token = await GetTokenAsync(cToken);

        //        var options = new RestClientOptions(_config.BaseUrl)
        //        {
        //            Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(token!, "Bearer"),
        //        };
        //        using var client = new RestClient(options);
        //        var request = new RestRequest() { Method = Method.Get };
        //        var response = await client.ExecuteGetAsync(request, cToken);

        //        if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
        //        {
        //            // Handle expired token
        //            if (response.ErrorException != null && response.ErrorException.ToString().Contains("Unauthorized")
        //                || response.Content != null && response.Content.Contains("The access token being passed has expired or is invalid."))
        //            {
        //                await GetTokenAsync(cToken, force: true);
        //                await Task.Delay(TimeSpan.FromSeconds(3), cToken);
        //                throw new Exception($"Token request call unauthorized. ResponseStatus: {response.StatusCode}");
        //            }

        //            // Back off when too many requests
        //            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        //            {
        //                if (i == 3)
        //                    i--;

        //                await Task.Delay(TimeSpan.FromSeconds(20), cToken);
        //                continue;
        //            }

        //            // All other failures
        //            await Task.Delay(TimeSpan.FromSeconds(3), cToken);
        //            throw new Exception($"Bad response! [{response.StatusCode}] {response.StatusDescription}");
        //        }

        //        // Handle success but null content return
        //        if (string.IsNullOrEmpty(response.Content) || response.Content.Contains("\"empty\":true"))
        //        {
        //            await Task.Delay(TimeSpan.FromSeconds(3), cToken);
        //            throw new Exception($"Returned empty data set!");
        //        }

        //        return JsonConvert.DeserializeObject<UserPrincipals>(response.Content)
        //               ?? throw new Exception("Failed to deserialize UserPrincipals object!");
        //    }
        //    catch (Exception ex)
        //    {
        //        if (i == 3)
        //            throw new Exception($"SchwabApiClient.GetUserPrincipals - {ex.Message}");

        //        await Task.Delay(TimeSpan.FromSeconds(3), cToken);
        //    }
        //}

        //_con.WriteLog(MessageSeverity.Warning, _logWindow, $"SchwabApiClient.GetUserPrincipals - fell through a hole in the world...");
        //return null;
    }
}