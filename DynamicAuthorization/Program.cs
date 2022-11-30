using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

var permissionList = new PermissionList();

// Add services to the container.

builder.Services.AddSingleton(permissionList);
builder.Services.AddSingleton<PermissionDatabase>();
builder.Services.AddSingleton<IAuthorizationHandler, DynamicAuthorizationHandler>();

builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie");

builder.Services.AddAuthorization(x =>
{
x.AddPolicy("dynamic", pb => pb
    .RequireAuthenticatedUser()
    .AddAuthenticationSchemes("cookie")
    .AddRequirements(new DynamicReuqirement())
    );
});

var app = builder.Build();

app.MapGet("/", (ClaimsPrincipal user) =>
{
    return user.Claims.Select(x => new { x.Type, x.Value });
});

app.MapGet("/login", () => Results.SignIn(
    new ClaimsPrincipal(
        new ClaimsIdentity(
            new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString()),
            },
            "cookie"
            )
        ),
        authenticationScheme: "cookie"
    )
);

app.MapGet("/secret/one", () => "one")
    .RequireAuthorization("dynamic")
    .WithTags("auth:secret/one", "aut:secret");

app.MapGet("/secret/two", () => "two")
    .RequireAuthorization("dynamic")
    .WithTags("auth:secret/two", "aut:secret");

app.MapGet("/secret/three", () => "three")
    .RequireAuthorization("dynamic")
    .WithTags("auth:secret/three", "aut:secret");

app.MapGet("/permissions", (PermissionList p) => p);

app.MapGet("/promote", (string permission, ClaimsPrincipal user, PermissionDatabase db) =>
{
    var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
    db.AddPermission(userId, permission);
});


// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

//app.MapRazorPages();

var endpoints = app as IEndpointRouteBuilder;
var source = endpoints.DataSources.First();
foreach(var se  in source.Endpoints)
{
    var authTags = se.Metadata.OfType<TagsAttribute>()
        .SelectMany(x => x.Tags)
        .Where(y => y.StartsWith("auth"));
    
    foreach(var authTag in authTags)
    {
        permissionList.Add(authTag);
    }
    
}

app.Run();
