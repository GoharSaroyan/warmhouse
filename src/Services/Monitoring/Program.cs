// Monitoring Service.
//
// Owns letting a homeowner view the current, live state of their home -
// as opposed to Telemetry's historical aggregation/reports. See
// docs/c4/container-to-be.puml for how this service fits into the
// overall architecture.

using MassTransit;
using Monitoring.Api.Application;
using Monitoring.Api.Domain;
using Monitoring.Api.Infrastructure;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var databaseUrl = GetEnv("DATABASE_URL", "postgres://postgres:postgres@localhost:5432/monitoring");
var port = GetEnv("PORT", "5005");
var rabbitMqHost = GetEnv("RABBITMQ_HOST", "localhost");
var rabbitMqUser = GetEnv("RABBITMQ_USER", "guest");
var rabbitMqPassword = GetEnv("RABBITMQ_PASSWORD", "guest");

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddSingleton(_ => NpgsqlDataSource.Create(ConvertPostgresUrlToConnectionString(databaseUrl)));
builder.Services.AddSingleton<ILiveStateRepository, LiveStateRepository>();
builder.Services.AddSingleton<MonitoringHandler>();

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<DeviceStateChangedConsumer>();

    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(rabbitMqHost, "/", h =>
        {
            h.Username(rabbitMqUser);
            h.Password(rabbitMqPassword);
        });
        cfg.ConfigureEndpoints(context);
    });
});

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

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "monitoring" }));

app.MapControllers();

app.Logger.LogInformation("Monitoring Service starting on {Address}", $"http://0.0.0.0:{port}");
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
