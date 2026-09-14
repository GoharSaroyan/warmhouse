// Heating Control Service.
//
// Owns turning heating on/off and tracking desired/actual state per
// room. See docs/c4/container-to-be.puml for how this service fits
// into the overall architecture.

using Heating.Api.Application;
using Heating.Api.Domain;
using Heating.Api.Infrastructure;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var databaseUrl = GetEnv("DATABASE_URL", "postgres://postgres:postgres@localhost:5432/heating");
var port = GetEnv("PORT", "5002");

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddSingleton(_ => NpgsqlDataSource.Create(ConvertPostgresUrlToConnectionString(databaseUrl)));
builder.Services.AddSingleton<IHeatingStateRepository, HeatingStateRepository>();
builder.Services.AddSingleton<HeatingCommandHandler>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dataSource = scope.ServiceProvider.GetRequiredService<NpgsqlDataSource>();
    await Schema.EnsureCreatedAsync(dataSource);
    app.Logger.LogInformation("Connected to database and verified schema");
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "heating" }));

app.MapControllers();

app.Logger.LogInformation("Heating Control Service starting on {Address}", $"http://0.0.0.0:{port}");
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
