using Microsoft.AspNetCore.DataProtection;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDataProtection();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<AuthService>();

var app = builder.Build();

app.Use((ctx, next) =>
{
    var idp = ctx.RequestServices.GetRequiredService<IDataProtectionProvider>();
    
    var protector = idp.CreateProtector("auth-cookie");

    var authCookie = ctx.Request.Headers.Cookie
        .FirstOrDefault(x => x.StartsWith("auth="));

    var protectedPayload = authCookie.Split("=").Last();

    var payload = protector.Unprotect(protectedPayload);

    var parts = payload.Split(':');

    var key = parts[0];
    var value = parts[1];

    var claims = new List<Claim>();
    claims.Add(new Claim(key, value));
    var identity = new ClaimsIdentity();
    ctx.User = new ClaimsPrincipal(identity);

    return next();
});

app.MapGet("/username", (HttpContext ctx) =>
{
    return ctx.User;
});

app.MapGet("/login", (AuthService auth) =>
{
    auth.SignIn();
    return "ok";
});


app.Run();
