using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.Extensions.Options;
using Refresh_Token;
using System.Net.Http.Headers;
using System.Text.Json;

public class RefreshTokenContext
{
    private readonly HttpClient _client;

    public RefreshTokenContext(IHttpClientFactory clientFactory)
	{
        _client = clientFactory.CreateClient();
    }

    public async Task<OAuthTokenResponse> RefreshTokenAsync(TokenInfo info, CancellationToken ct)
    {
        var tokenRequestParameters = new Dictionary<string, string>()
        {
            {"client_id", PatreonOauthConfig.ClientId },
            {"client_secret", PatreonOauthConfig.ClientSecret },
            {"grant_type", "refresh_token" },
            {"refresh_token", info.RefreshToken}
        };

        var requestContent = new FormUrlEncodedContent(tokenRequestParameters);
        var requestMessage = new HttpRequestMessage(HttpMethod.Post, PatreonOauthConfig.ToKenEndPoint);
        requestMessage.Headers.Accept.Add(new 
            MediaTypeWithQualityHeaderValue("application/json"));
        requestMessage.Content = requestContent;
        requestMessage.Version = _client.DefaultRequestVersion;
        var response = await _client.SendAsync(requestMessage, ct);
        var body = await response.Content.ReadAsStringAsync(ct);

        return  OAuthTokenResponse.Success(JsonDocument.Parse(body));
    }
}
