// API Gateway - single public entry point for the WarmHouse platform.
//
// Routes each incoming request to the microservice that owns it, based
// on path prefix, using YARP. This is the only container the web/mobile
// client talks to directly - see docs/c4/container-to-be.puml.
//
// Route -> backend service mapping lives in appsettings.json
// (ReverseProxy:Routes / ReverseProxy:Clusters) so it can be changed
// without a code change or recompile.

var builder = WebApplication.CreateBuilder(args);

var port = GetEnv("PORT", "5000");
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services
    .AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "api-gateway" }));

app.MapReverseProxy();

app.Logger.LogInformation("API Gateway starting on {Address}", $"http://0.0.0.0:{port}");
app.Run();

return;

static string GetEnv(string key, string defaultValue)
{
    var value = Environment.GetEnvironmentVariable(key);
    return string.IsNullOrEmpty(value) ? defaultValue : value;
}
