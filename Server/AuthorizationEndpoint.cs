using Microsoft.AspNetCore.DataProtection;
using System.Text.Json;
using System.Web;

namespace Server
{
    public static class AuthorizationEndpoint
    {
        public static IResult Handle(HttpRequest request, IDataProtectionProvider provider)
        {
            var issue = HttpUtility.UrlEncode("https://localhost:5001");

            request.Query.TryGetValue("state", out var state);

            if (!request.Query.TryGetValue("response_type", out var responseType))
            {
                return Results.BadRequest(
                    new
                    {
                        error = "invalid_request",
                        state,
                        issue
                    }
               );
            }
            //request.Query.TryGetValue("response_type", out var responseType);
            request.Query.TryGetValue("client_id", out var clientId);
            request.Query.TryGetValue("code_challenge", out var codeChallenge);
            request.Query.TryGetValue("code_challenge_method", out var codeChallengeMethod);
            request.Query.TryGetValue("redirect_uri", out var redirectUri);
            request.Query.TryGetValue("scope", out var scope);
            
            var dataProtector = provider.CreateProtector("oauth");
            var code = new AuthCode
            {
                ClientId= clientId,
                CodeChallenge= codeChallenge,
                CodeChallengeMethod = codeChallengeMethod,
                RedirectUri= redirectUri,
                Expiry = DateTime.Now.AddMinutes(5)
            };

            var coderString = dataProtector.Protect(JsonSerializer.Serialize(code));

            return Results.Redirect(
                $"{redirectUri}?code={coderString}&state={state}&iss={HttpUtility.UrlEncode("https://localhost:5001")}");

        }
    }
}
