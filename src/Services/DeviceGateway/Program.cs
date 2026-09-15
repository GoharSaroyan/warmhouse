// Device Gateway.
//
// The only container that talks to physical/partner devices (see
// docs/c4/container-to-be.puml). Consumes command-requested events from
// the control services over RabbitMQ, "delivers" them, and publishes
// the ack plus the resulting device-state-change event. Has no database
// of its own - it's a stateless protocol adapter.

using DeviceGateway.Api.Consumers;
using MassTransit;

var builder = WebApplication.CreateBuilder(args);

var port = GetEnv("PORT", "5009");
var rabbitMqHost = GetEnv("RABBITMQ_HOST", "localhost");
var rabbitMqUser = GetEnv("RABBITMQ_USER", "guest");
var rabbitMqPassword = GetEnv("RABBITMQ_PASSWORD", "guest");

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<HeatingCommandRequestedConsumer>();
    x.AddConsumer<LightingCommandRequestedConsumer>();
    x.AddConsumer<AccessCommandRequestedConsumer>();

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

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "device-gateway" }));

app.Logger.LogInformation("Device Gateway starting on {Address}", $"http://0.0.0.0:{port}");
app.Run();

return;

static string GetEnv(string key, string defaultValue)
{
    var value = Environment.GetEnvironmentVariable(key);
    return string.IsNullOrEmpty(value) ? defaultValue : value;
}
