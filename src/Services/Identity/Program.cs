// User Identity Service.
//
// Owns homeowner accounts, authentication and home/tenant membership.
// See docs/c4/container-to-be.puml for how this service fits into the
// overall architecture.

using Identity.Api.Application;
using Identity.Api.Domain;
using Identity.Api.Infrastructure;
using Npgsql;
using WarmHouse.WebDefaults;

var builder = WebApplication.CreateBuilder(args);

var databaseUrl = GetEnv("DATABASE_URL", "postgres://postgres:postgres@localhost:5432/identity");
var port = GetEnv("PORT", "5007");

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddSingleton(_ => NpgsqlDataSource.Create(ConvertPostgresUrlToConnectionString(databaseUrl)));
builder.Services.AddSingleton<IUserRepository, UserRepository>();
builder.Services.AddSingleton<IHouseRepository, HouseRepository>();
builder.Services.AddSingleton<UserHandler>();
builder.Services.AddSingleton<HouseHandler>();

builder.Services.AddControllers();
builder.AddApiDocumentation("User Identity Service API", "Owns homeowner accounts, authentication and home/tenant membership.");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dataSource = scope.ServiceProvider.GetRequiredService<NpgsqlDataSource>();
    await Schema.EnsureCreatedAsync(dataSource);
    app.Logger.LogInformation("Connected to database and verified schema");
}

app.UseApiDocumentation("User Identity Service API");

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "identity" }));

app.MapControllers();

app.Logger.LogInformation("User Identity Service starting on {Address}", $"http://0.0.0.0:{port}");
app.Run();

return;

static string GetEnv(string key, string defaultValue)
{
    var value = Environment.GetEnvironmentVariable(key);
    return string.IsNullOrEmpty(value) ? defaultValue : value;
}

static string ConvertPostgresUrlToConnectionString(string url)
{
    var uri = new Uri(url);
    var userInfo = uri.UserInfo.Split(':', 2);
    var database = uri.AbsolutePath.TrimStart('/');

    return new NpgsqlConnectionStringBuilder
    {
        Host = uri.Host,
        Port = uri.Port > 0 ? uri.Port : 5432,
        Username = userInfo.ElementAtOrDefault(0),
        Password = userInfo.ElementAtOrDefault(1),
        Database = database,
    }.ConnectionString;
}
