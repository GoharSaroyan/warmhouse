// Telemetry Service - skeleton.
//
// Owns ingesting, storing and aggregating device data over time for
// historical analysis and reports. See docs/c4/component-telemetry.puml
// for the intended internal structure (Presentation/Application/Domain/
// Infrastructure) - this skeleton just gets it running behind the
// gateway until that structure is implemented.

var builder = WebApplication.CreateBuilder(args);

var port = GetEnv("PORT", "5006");
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "telemetry" }));

app.MapControllers();

app.Logger.LogInformation("Telemetry Service (skeleton) starting on {Address}", $"http://0.0.0.0:{port}");
app.Run();

return;

static string GetEnv(string key, string defaultValue)
{
    var value = Environment.GetEnvironmentVariable(key);
    return string.IsNullOrEmpty(value) ? defaultValue : value;
}
