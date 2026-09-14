using Npgsql;
using SmartHome.Api.Data;
using SmartHome.Api.Services;

var builder = WebApplication.CreateBuilder(args);

var databaseUrl = GetEnv("DATABASE_URL", "postgres://postgres:postgres@localhost:5432/smarthome");
var temperatureApiUrl = GetEnv("TEMPERATURE_API_URL", "http://temperature-api:8081");
var port = GetEnv("PORT", "8080");

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// A single pooled connection source, opened once and shared across requests.
builder.Services.AddSingleton(_ =>
{
    var connectionString = ConvertPostgresUrlToConnectionString(databaseUrl);
    return NpgsqlDataSource.Create(connectionString);
});
builder.Services.AddSingleton<SensorRepository>();

builder.Services.AddHttpClient<TemperatureService>(client =>
{
    client.BaseAddress = new Uri(temperatureApiUrl);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

// Verify the database connection at startup.
using (var scope = app.Services.CreateScope())
{
    var dataSource = scope.ServiceProvider.GetRequiredService<NpgsqlDataSource>();
    await using var conn = await dataSource.OpenConnectionAsync();
    app.Logger.LogInformation("Connected to database successfully");
}

app.Logger.LogInformation("Temperature service initialized with API URL: {Url}", temperatureApiUrl);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Health check endpoint.
app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapControllers();

app.Logger.LogInformation("Server starting on {Address}", $"http://0.0.0.0:{port}");
app.Run();

return;

// getEnv gets an environment variable or returns a default value.
static string GetEnv(string key, string defaultValue)
{
    var value = Environment.GetEnvironmentVariable(key);
    return string.IsNullOrEmpty(value) ? defaultValue : value;
}

// Npgsql wants a keyword/value connection string rather than a postgres:// URL.
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
