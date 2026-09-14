// Telemetry Service.
//
// Owns ingesting, storing and aggregating device data over time for
// historical analysis and reports - as opposed to Monitoring's live
// state. See docs/c4/component-telemetry.puml for the intended
// structure; this is a raw-Npgsql implementation of the same layering
// rather than EF Core, to match the rest of these services.

using Npgsql;
using Telemetry.Api.Application;
using Telemetry.Api.Domain;
using Telemetry.Api.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

var databaseUrl = GetEnv("DATABASE_URL", "postgres://postgres:postgres@localhost:5432/telemetry");
var port = GetEnv("PORT", "5006");

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddSingleton(_ => NpgsqlDataSource.Create(ConvertPostgresUrlToConnectionString(databaseUrl)));

builder.Services.AddSingleton<ITelemetryPointRepository, TelemetryPointRepository>();
builder.Services.AddSingleton<IThresholdRuleRepository, ThresholdRuleRepository>();
builder.Services.AddSingleton<ITelemetryQueries, TelemetryQueries>();

builder.Services.AddSingleton<RecordMeasurementHandler>();
builder.Services.AddSingleton<CreateThresholdRuleHandler>();

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

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "telemetry" }));

app.MapControllers();

app.Logger.LogInformation("Telemetry Service starting on {Address}", $"http://0.0.0.0:{port}");
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
