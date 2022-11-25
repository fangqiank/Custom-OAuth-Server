using Server;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddAuthentication("cookie")
    .AddCookie("cookie", o =>
    {
        o.LoginPath = "/login";
    });
builder.Services.AddAuthorization();
builder.Services.AddSingleton<DevKeys>();

var app = builder.Build();

app.MapGet("/login", GetLogin.Handler);
app.MapPost("/login", Login.Handler);
app.MapGet("/oauth/authorize", AuthorizationEndpoint.Handle)
    .RequireAuthorization();
app.MapPost("/oauth/token", TokenEndpoint.Handle);

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
