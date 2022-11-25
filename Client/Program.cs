using Microsoft.AspNetCore.Authentication;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie")
    .AddOAuth("custom", o =>
    {
        o.SignInScheme = "cookie";

        o.ClientId = "x";
        o.ClientSecret= "y";

        o.AuthorizationEndpoint = "https://localhost:5001/oauth/authorize";
        o.TokenEndpoint = "https://localhost:5001/oauth/token";
        o.CallbackPath= "/oauth/custom-cb";

        o.UsePkce= true;
        o.ClaimActions.MapJsonKey("sub", "sub");
        o.ClaimActions.MapJsonKey("custom hello", "custom");
        o.Events.OnCreatingTicket = async ctx =>
        {
            //map claims
            var payloadBase64 = ctx.AccessToken.Split('.')[1];
            var payloadJson = Base64UrlTextEncoder.Decode(payloadBase64);
            var payload = JsonDocument.Parse(payloadJson);
            ctx.RunClaimActions(payload.RootElement);

        };
    });

var app = builder.Build();

app.MapGet("/", (HttpContext ctx) =>
{
    return ctx.User.Claims
    .Select(c => new { c.Type, c.Value })
    .ToList();
});

app.MapGet("/login", () =>
{
    return Results.Challenge(
        new AuthenticationProperties()
        {
            RedirectUri = "https://localhost:6001"
        },
        authenticationSchemes: new List<string>() {"custom"}
   ) ;
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

//app.UseAuthentication();
//app.UseAuthorization();

app.Run();
