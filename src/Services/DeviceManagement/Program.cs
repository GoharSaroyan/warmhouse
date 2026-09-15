using DeviceManagement.Api.Application;
using DeviceManagement.Api.Domain;
using DeviceManagement.Api.Infrastructure;
using Npgsql;
using WarmHouse.WebDefaults;

var builder = WebApplication.CreateBuilder(args);

var databaseUrl = GetEnv("DATABASE_URL", "postgres://postgres:postgres@localhost:5432/device_management");
var port = GetEnv("PORT", "5001");

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// A single pooled connection source, opened once and shared across requests.
builder.Services.AddSingleton(_ =>
{
    var connectionString = ConvertPostgresUrlToConnectionString(databaseUrl);
    return NpgsqlDataSource.Create(connectionString);
});

// Infrastructure (adapters).
builder.Services.AddSingleton<IDeviceTypeRepository, DeviceTypeRepository>();
builder.Services.AddSingleton<IModuleRepository, ModuleRepository>();
builder.Services.AddSingleton<IDeviceRepository, DeviceRepository>();
builder.Services.AddSingleton<IDeviceGatewayClient, DeviceGatewayClient>();

// Domain / Application.
builder.Services.AddSingleton<DeviceStateManager>();
builder.Services.AddSingleton<CommandHandler>();
builder.Services.AddSingleton<OnboardingHandler>();

// ASP.NET Core's default JSON policy (camelCase) applies uniformly to
// every DTO here - no per-property [JsonPropertyName] attributes needed.
builder.Services.AddControllers();
builder.AddApiDocumentation("Device Management Service API", "Device catalog, self-service onboarding and ownership.");

var app = builder.Build();

// Verify the database connection and schema at startup.
using (var scope = app.Services.CreateScope())
{
    var dataSource = scope.ServiceProvider.GetRequiredService<NpgsqlDataSource>();
    await Schema.EnsureCreatedAsync(dataSource);
    app.Logger.LogInformation("Connected to database and verified schema");
}

app.UseApiDocumentation("Device Management Service API");

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "device-management" }));

app.MapControllers();

app.Logger.LogInformation("Device Management Service starting on {Address}", $"http://0.0.0.0:{port}");
app.Run();

return;

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
