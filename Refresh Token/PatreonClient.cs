using Polly;
using System.Net;
using System.Net.Http.Headers;

namespace Refresh_Token
{
    public class PatreonClient
    {
        private readonly HttpClient _client;
        private readonly TokenDatabase _tokenDatabase;

        public PatreonClient(HttpClient client, TokenDatabase tokenDatabase)
        {
            _client = client;
            _tokenDatabase = tokenDatabase;
        }

        public async Task<string> GetInfo(string customId)
        {
            var tokenInfo = _tokenDatabase.Get(customId);
            using var request = new HttpRequestMessage(HttpMethod.Get, 
                PatreonOauthConfig.UserInformationEndPoint);
            request.Headers.Add("customId", customId);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",
                tokenInfo.AccessToken);

            using var result = await _client.SendAsync(request);
            return await result.Content.ReadAsStringAsync();
        }

        public static IAsyncPolicy<HttpResponseMessage> HandleUnauthorized(
            IServiceProvider serviceProvider, 
            HttpRequestMessage httpRequestMessage
            )
        {
            return Policy.HandleResult<HttpResponseMessage>(response =>
                response.StatusCode == HttpStatusCode.Unauthorized)
                .RetryAsync(1, async(e, attempt) =>
                {
                    using var scope = serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<TokenDatabase>();
                    var context = scope.ServiceProvider.GetRequiredService<RefreshTokenContext>();
                    var customId = httpRequestMessage.Headers.GetValues("customId").First();
                    var tokenInfo = db.Get(customId);
                    var result = await context.RefreshTokenAsync(tokenInfo, CancellationToken.None);

                    db.Save(customId, new TokenInfo(
                        result.AccessToken,
                        result.RefreshToken,
                        DateTime.UtcNow.AddSeconds(int.Parse(result.ExpiresIn))
                        ));

                    httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue(
                        "Bearer", result.AccessToken);

                });
        }

    }
}
