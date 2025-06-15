using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using RestSharp;
using RestSharp.Authenticators;
using StonkBot.Appsettings.Models;

namespace StonkBot.Services.CharlesSchwab.APIClient.Models;

record TokenResponse
{
    [JsonPropertyName("token_type")]
    public string? TokenType { get; init; }
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; init; }
}

public class SchwabAuthenticator : AuthenticatorBase
{
    private readonly SchwabApiConfig _config;

    public SchwabAuthenticator(IOptions<SchwabApiConfig> schwabConfig) : base("")
    {
        _config = schwabConfig.Value;
    }

    protected override async ValueTask<Parameter> GetAuthenticationParameter(string accessToken)
    {
        Token = string.IsNullOrEmpty(Token) ? await GetToken() : Token;
        return new HeaderParameter(KnownHeaders.Authorization, Token);
    }

    async Task<string> GetToken()
    {
        var options = new RestClientOptions(_config.BaseUrl)
        {
            Authenticator = new HttpBasicAuthenticator(_config.AppKey, _config.AppSecret),
        };
        using var client = new RestClient(options);

        var request = new RestRequest(_config.TokenUrl).AddParameter("grant_type", "client_credentials");

        var response = await client.PostAsync<TokenResponse>(request);
        return $"{response!.TokenType} {response!.AccessToken}";
    }
}