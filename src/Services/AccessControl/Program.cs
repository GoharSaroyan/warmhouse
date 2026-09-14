// Access Control Service.
//
// Owns locking/unlocking automatic gates and the access audit trail.
// See docs/c4/container-to-be.puml for how this service fits into the
// overall architecture.

using AccessControl.Api.Application;
using AccessControl.Api.Domain;
using AccessControl.Api.Infrastructure;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

var databaseUrl = GetEnv("DATABASE_URL", "postgres://postgres:postgres@localhost:5432/access_control");
var port = GetEnv("PORT", "5004");

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddSingleton(_ => NpgsqlDataSource.Create(ConvertPostgresUrlToConnectionString(databaseUrl)));
builder.Services.AddSingleton<IAccessStateRepository, AccessStateRepository>();
builder.Services.AddSingleton<IAccessAuditRepository, AccessAuditRepository>();
builder.Services.AddSingleton<AccessCommandHandler>();

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

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "access-control" }));

app.MapControllers();

app.Logger.LogInformation("Access Control Service starting on {Address}", $"http://0.0.0.0:{port}");
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
