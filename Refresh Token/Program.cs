using Microsoft.AspNetCore.Authentication;
using Refresh_Token;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddSingleton<TokenDatabase>()
            .AddHostedService<TokenRefresh>()
            .AddTransient<RefreshTokenContext>()
            .AddHttpClient()
            .AddHttpClient<PatreonClient>()
            .AddPolicyHandler(PatreonClient.HandleUnauthorized);

        builder.Services.AddAuthentication("cookie")
            .AddCookie("cookie")
            .AddOAuth("patreon", o =>
            {
                o.SignInScheme = "cookie";

                o.ClientId = PatreonOauthConfig.ClientId;
                o.ClientSecret = PatreonOauthConfig.ClientSecret;

                o.AuthorizationEndpoint = PatreonOauthConfig.AuthorizationEndpoint;
                o.TokenEndpoint = PatreonOauthConfig.ToKenEndPoint;
                o.UserInformationEndpoint = PatreonOauthConfig.UserInformationEndPoint;
                o.CallbackPath = "/oauth/patreon-cb";
                o.SaveTokens = false;

                o.Scope.Clear();
                o.Scope.Add("identity");

                o.Events.OnCreatingTicket = async ctx =>
                {
                    //map claims
                    var database = ctx.HttpContext.RequestServices
                    .GetRequiredService<TokenDatabase>();

                    using var request = new HttpRequestMessage(HttpMethod.Get,
                        ctx.Options.UserInformationEndpoint);
                    request.Headers.Authorization = new AuthenticationHeaderValue("Bearer",
                        ctx.AccessToken);

                    using var result = await ctx.Backchannel.SendAsync(request);
                    var content = await result.Content.ReadAsStringAsync();
                    var userJson = JsonDocument.Parse(content).RootElement;

                    var customId = userJson.GetProperty("data").GetProperty("id")
                        .GetString();
                    database.Save(customId, new TokenInfo(
                        ctx.AccessToken,
                        ctx.RefreshToken,
                        DateTime.UtcNow.AddSeconds(int.Parse(ctx.TokenResponse.ExpiresIn))
                   ));

                    ctx.Identity.AddClaim(new Claim("customId", customId));

                };
            });

        builder.Services.AddAuthorization();

        var app = builder.Build();

        app.MapGet("/", (ClaimsPrincipal user) => user.Claims.Select(x => new {x.Type, x.Value})
            .ToList());

        app.MapGet("/login", () => Results.Challenge(
            new AuthenticationProperties
            {
                RedirectUri = "/"
            },
            new List<string>() {"patreon"}
        ));

        app.MapGet("/info", (PatreonClient patreonClient, ClaimsPrincipal user) =>
        {
            var patreonId = user.FindFirstValue("customId");
            return patreonClient.GetInfo(patreonId);
        });

        // Configure the HTTP request pipeline.
        //if (!app.Environment.IsDevelopment())
        //{
        //    app.UseExceptionHandler("/Error");
        //    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        //    app.UseHsts();
        //}

        //app.UseHttpsRedirection();
        //app.UseStaticFiles();

        //app.UseRouting();

        //app.UseAuthorization();

        app.Run();
    }
}