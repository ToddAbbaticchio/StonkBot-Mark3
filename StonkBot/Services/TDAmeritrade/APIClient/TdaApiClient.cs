using System.Net;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using RestSharp;
using RestSharp.Authenticators.OAuth2;
using StonkBot.Data;
using StonkBot.Services.ConsoleWriter;
using StonkBot.Services.ConsoleWriter.Enums;
using StonkBot.Services.TDAmeritrade._SharedModels;
using StonkBot.Services.TDAmeritrade.APIClient.Models;

namespace StonkBot.Services.TDAmeritrade.APIClient;

public interface ITdaApiClient
{
    Task<string?> GetTokenAsync(bool force, CancellationToken cToken);
    Task<Quote?> GetQuoteAsync(string symbol, CancellationToken cToken);
    Task<List<Quote?>> GetQuotesAsync(List<string> symbols, CancellationToken cToken);
    Task<HistoricalDataResponse?> GetHistoricalDataAsync(string symbol, string periodType, string period, string frequencyType, CancellationToken cToken);
    Task<UserPrincipals?> GetUserPrincipals(CancellationToken cToken);
}

public class TdaApiClient : ITdaApiClient
{
    private readonly IConsoleWriter _con;
    private readonly IStonkBotDb _db;
    private readonly TargetLog _logWindow;

    public TdaApiClient(IConsoleWriter con, IStonkBotDb db)
    {
        _con = con;
        _db = db;
        _logWindow = TargetLog.ActionRunner;
    }

    public async Task<string?> GetTokenAsync(bool force, CancellationToken cToken)
    {
        try
        {
            //await using var _db = new StonkBotDbContext();

            var dbToken = await _db.AuthTokens.FirstOrDefaultAsync(cToken);
            if (dbToken == null)
                throw new Exception("No TDAmeritrade auth token found!");

            var now = DateTime.UtcNow;
            var refreshTime = dbToken.tokenCreatedTime + TimeSpan.FromSeconds(dbToken.expires_in - 300);

            if (force == false && (dbToken.expires_in != 0 || refreshTime >= now))
                return dbToken.access_token;

            // If not valid get a new token using the dbToken refreshtoken
            RestResponse response;
            try
            {
                using var client = new RestClient(Constants.TDTokenUrl);
                var request = new RestRequest() { Method = Method.Post };
                var requestParams = new { grant_type = "refresh_token", dbToken.refresh_token, client_id = Constants.TDAmeritradeClientId };
                request.AddObject(requestParams);
                response = await client.ExecuteAsync(request, cToken);
                if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
                    throw new Exception($"Failed to acquire a new TDA access token: {response.ResponseStatus}");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error acquiring new access token from TDA API: {ex.Message}");
            }

            // Deserialize the TDA response and store new token info
            try
            {
                var rToken = JsonConvert.DeserializeObject<AccessTokenRefreshResponse>(response.Content);
                if (rToken == null || string.IsNullOrEmpty(rToken.access_token))
                    throw new Exception("Failed to deserialize tdameritrade authtoken response!");

                dbToken.tokenCreatedTime = rToken.tokenCreatedTime;
                dbToken.access_token = rToken.access_token;
                dbToken.expires_in = rToken.expires_in;
                dbToken.scope = rToken.scope;
                dbToken.token_type = rToken.token_type;

                //var asdf = dbTokens.Update(dbToken);
                await _db.SbSaveChangesAsync(cToken);

                return rToken.access_token;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error saving new access token: {ex.Message}");
            }
        }
        catch (Exception ex)
        {
            _con.WriteLog(MessageSeverity.Error, $"TdaApiService.GetTokenAsync: {ex.Message}");
            throw new Exception(ex.Message);
        }
    }

    public async Task<Quote?> GetQuoteAsync(string symbol, CancellationToken cToken)
    {
        try
        {
            var url = Constants.TDQuoteUrl.Replace("PLACEHOLDER", symbol);

            for (var i = 1; i <= 3; i++)
            {
                try
                {
                    var token = await GetTokenAsync(false, cToken);

                    using var client = new RestClient(url);
                    client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(token!, "Bearer");
                    var request = new RestRequest { Method = Method.Get };
                    var response = await client.ExecuteGetAsync(request, cToken);
                    switch (response.StatusCode)
                    {
                        case HttpStatusCode.OK:
                        {
                            break;
                        }
                        
                        case HttpStatusCode.Unauthorized:
                        {
                            token = await GetTokenAsync(true, cToken);
                            await Task.Delay(TimeSpan.FromSeconds(3), cToken);
                            throw new Exception($"Bad response! [{response.StatusCode}] {response.StatusDescription}");
                        }

                        default:
                        {
                            throw new Exception($"Bad response for {symbol} quote! [{response.StatusCode}] {response.StatusDescription}");
                        }
                    }

                    if (string.IsNullOrEmpty(response.Content))
                        throw new Exception($"Recieved an empty dataset in response for {symbol}!");

                    var quote = JsonConvert.DeserializeObject<QuoteList>(response.Content);
                    if (quote?.Info == null)
                        throw new Exception($"Failed to deserialize quote response for {symbol}!");
                    if (!quote.Info.Any())
                        throw new Exception($"Deserialized quote response for {symbol} empty!");

                    return quote.Info!.FirstOrDefault()!;
                }
                catch (Exception ex)
                {
                    if (i == 3)
                    {
                        _con.WriteLog(MessageSeverity.Warning, $"TdaApiClient.GetQuoteAsync - {ex.Message}");
                        return null;
                    }
                    await Task.Delay(TimeSpan.FromSeconds(3), cToken);
                }
            }

            _con.WriteLog(MessageSeverity.Warning, _logWindow, $"TdaApiClient.GetQuoteAsync - {symbol} fell through a hole in the world...");
            return null;
        }
        catch (Exception ex)
        {
            _con.WriteLog(MessageSeverity.Error, $"TdaApiClient.GetQuoteAsync - Unhandled exception for {symbol}: {ex.Message}");
            return null;
        }
    }

    public async Task<List<Quote?>> GetQuotesAsync(List<string> symbols, CancellationToken cToken)
    {
        try
        {
            var url = Constants.TDMultiQuoteUrl.Replace("PLACEHOLDER", string.Join("%2C", symbols));

            for (var i = 1; i <= 3; i++)
            {
                try
                {
                    var token = await GetTokenAsync(false, cToken);

                    using var client = new RestClient(url);
                    client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(token!, "Bearer");
                    var request = new RestRequest() { Method = Method.Get };
                    var response = await client.ExecuteGetAsync(request, cToken);
                    if (!response.IsSuccessful)
                    {
                        if (response.ErrorException != null && response.ErrorException.ToString().Contains("Unauthorized"))
                        {
                            token = await GetTokenAsync(true, cToken);
                            await Task.Delay(TimeSpan.FromSeconds(3), cToken);
                            throw new Exception($"Attempt {i} of 3 - TDA Api call unauthorized. Expiring TDAToken...");
                        }

                        throw new Exception($"Failed to acquire multi-quote response from tdaApi: {response.ErrorMessage}");
                    }

                    if (string.IsNullOrEmpty(response.Content))
                        throw new Exception("Multi-quote response.Content is null or empty!");

                    var quotes = JsonConvert.DeserializeObject<QuoteList>(response.Content);
                    if (quotes!.Info == null)
                        throw new Exception("Failed to deserialize multi quote response!");
                    if (!quotes.Info.Any())
                        throw new Exception("Deserialized multi quote response is null or empty!");

                    return quotes.Info!;
                }
                catch (Exception ex)
                {
                    _con.WriteLog(MessageSeverity.Warning, $"Attempt {i} of 3 - Error: {ex.Message}");
                    await Task.Delay(TimeSpan.FromSeconds(3), cToken);
                }
            }
        }
        catch (Exception ex)
        {
            _con.WriteLog(MessageSeverity.Warning, $"Error acquiring multi stonk quote: {ex.Message}");
            throw;
        }

        throw new Exception("Unhandled exception at end of GetQuotesAsync! Investigate this!");
    }

    public async Task<HistoricalDataResponse?> GetHistoricalDataAsync(string symbol, string periodType, string period, string frequencyType, CancellationToken cToken)
    {
        var url = Constants.TDHistoricalQuoteUrl
            .Replace("SYMBOL", symbol)
            .Replace("PERIODTYPE", periodType)
            .Replace("PERIOD", period)
            .Replace("FREQUENCYTYPE", frequencyType);

        for (var i = 1; i <= 3; i++)
        {
            try
            {
                var token = await GetTokenAsync(false, cToken);

                using var client = new RestClient(url);
                client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(token!, "Bearer");
                var request = new RestRequest() { Method = Method.Get };
                var response = await client.ExecuteGetAsync(request, cToken);

                if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
                {
                    // Handle expired token
                    if (response.ErrorException != null && response.ErrorException.ToString().Contains("Unauthorized")
                        || response.Content != null && response.Content.Contains("The access token being passed has expired or is invalid."))
                    {
                        if (i == 3)
                            throw new Exception($"{symbol} attempt {i} of 3 - TDA Api call unauthorized. ResponseStatus: {response.StatusCode}");

                        token = await GetTokenAsync(true, cToken);
                        await Task.Delay(TimeSpan.FromSeconds(3), cToken);
                        continue;
                    }

                    // Back off when too many requests
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        if (i == 3)
                            i--;

                        await Task.Delay(TimeSpan.FromSeconds(20), cToken);
                        continue;
                    }

                    // All other failures
                    if (i == 3)
                        throw new Exception($"{symbol} attempt {i} of 3 failed. ResponseStatus: {response.StatusCode}  ResponseException: {response.ErrorException}");

                    await Task.Delay(TimeSpan.FromSeconds(3), cToken);
                    continue;
                }

                // Handle success but null content return
                if (string.IsNullOrEmpty(response.Content) || response.Content.Contains("\"empty\":true"))
                {
                    if (i == 3)
                        throw new Exception($"{symbol} attempt {i} of 3 success - but TDA returned an empty data table.");

                    await Task.Delay(TimeSpan.FromSeconds(3), cToken);
                    continue;
                }

                // Handle failure to dezerialize or empty dataset
                var history = JsonConvert.DeserializeObject<HistoricalDataResponse>(response.Content);
                if (history == null || !history.candles.Any())
                {
                    if (i == 3)
                        throw new Exception($"{symbol} attempt {i} of 3 success - but the returned data was unable to be deserialized.");

                    await Task.Delay(TimeSpan.FromSeconds(3), cToken);
                    continue;
                }

                history.candles.ForEach(x => x.GoodDateTime = DateTimeOffset.FromUnixTimeMilliseconds(x.datetime).Date);
                return history;
            }
            catch (Exception ex)
            {
                if (i == 3)
                    throw new Exception(ex.Message);

                await Task.Delay(TimeSpan.FromSeconds(3), cToken);
            }
        }

        throw new Exception($"Unhandled exception at end of GetHistoricalDataAsync for {symbol}.");
    }

    public async Task<UserPrincipals?> GetUserPrincipals(CancellationToken cToken)
    {
        for (var i = 1; i <= 3; i++)
        {
            try
            {
                var token = await GetTokenAsync(false, cToken);

                using var client = new RestClient(Constants.TDAmeritradeUserPrincipalsUrl);
                client.Authenticator = new OAuth2AuthorizationRequestHeaderAuthenticator(token!, "Bearer");
                var request = new RestRequest() { Method = Method.Get };
                var response = await client.ExecuteGetAsync(request, cToken);

                if (!response.IsSuccessful || string.IsNullOrEmpty(response.Content))
                {
                    // Handle expired token
                    if (response.ErrorException != null && response.ErrorException.ToString().Contains("Unauthorized")
                        || response.Content != null && response.Content.Contains("The access token being passed has expired or is invalid."))
                    {
                        await GetTokenAsync(true, cToken);
                        await Task.Delay(TimeSpan.FromSeconds(3), cToken);
                        throw new Exception($"Token request call unauthorized. ResponseStatus: {response.StatusCode}");
                    }

                    // Back off when too many requests
                    if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                    {
                        if (i == 3)
                            i--;

                        await Task.Delay(TimeSpan.FromSeconds(20), cToken);
                        continue;
                    }

                    // All other failures
                    await Task.Delay(TimeSpan.FromSeconds(3), cToken);
                    throw new Exception($"Bad response! [{response.StatusCode}] {response.StatusDescription}");
                }

                // Handle success but null content return
                if (string.IsNullOrEmpty(response.Content) || response.Content.Contains("\"empty\":true"))
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), cToken);
                    throw new Exception($"Returned empty data set!");
                }

                return JsonConvert.DeserializeObject<UserPrincipals>(response.Content)
                       ?? throw new Exception("Failed to deserialize UserPrincipals object!");
            }
            catch (Exception ex)
            {
                if (i == 3)
                    throw new Exception($"TdaApiClient.GetUserPrincipals - {ex.Message}");

                await Task.Delay(TimeSpan.FromSeconds(3), cToken);
            }
        }

        _con.WriteLog(MessageSeverity.Warning, _logWindow, $"TdaApiClient.GetUserPrincipals - fell through a hole in the world...");
        return null;
    }
}