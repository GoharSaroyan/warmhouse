// Heating Control Service - skeleton.
//
// Owns turning heating on/off and tracking desired/actual state per room.
// See docs/c4/container-to-be.puml for how this service fits into the
// overall architecture (consumes commands published to the Message
// Broker by the API Gateway path, publishes state-change/ack events back).
//
// Not yet implemented - this is a placeholder so the API Gateway has a
// real backend to route to. Fill in Controllers/, a DbContext and the
// command-handling pipeline when this service is built out.

var builder = WebApplication.CreateBuilder(args);

var port = GetEnv("PORT", "5002");
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "heating" }));

app.MapControllers();

app.Logger.LogInformation("Heating Control Service (skeleton) starting on {Address}", $"http://0.0.0.0:{port}");
app.Run();

return;

static string GetEnv(string key, string defaultValue)
{
    var value = Environment.GetEnvironmentVariable(key);
    return string.IsNullOrEmpty(value) ? defaultValue : value;
}
