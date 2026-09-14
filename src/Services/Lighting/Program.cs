// Lighting Control Service - skeleton.
//
// Owns turning lights on/off per room. See docs/c4/container-to-be.puml
// for how this service fits into the overall architecture.
//
// Not yet implemented - placeholder so the API Gateway has a real
// backend to route to.

var builder = WebApplication.CreateBuilder(args);

var port = GetEnv("PORT", "5003");
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "lighting" }));

app.MapControllers();

app.Logger.LogInformation("Lighting Control Service (skeleton) starting on {Address}", $"http://0.0.0.0:{port}");
app.Run();

return;

static string GetEnv(string key, string defaultValue)
{
    var value = Environment.GetEnvironmentVariable(key);
    return string.IsNullOrEmpty(value) ? defaultValue : value;
}
