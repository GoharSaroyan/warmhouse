// Billing Service.
//
// Owns the SaaS self-service subscription and module purchase/
// entitlement per home. See docs/c4/container-to-be.puml for how this
// service fits into the overall architecture.

using Billing.Api.Application;
using Billing.Api.Domain;
using Billing.Api.Infrastructure;
using Npgsql;
using WarmHouse.WebDefaults;

var builder = WebApplication.CreateBuilder(args);

var databaseUrl = GetEnv("DATABASE_URL", "postgres://postgres:postgres@localhost:5432/billing");
var port = GetEnv("PORT", "5008");

builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

builder.Services.AddSingleton(_ => NpgsqlDataSource.Create(ConvertPostgresUrlToConnectionString(databaseUrl)));
builder.Services.AddSingleton<ISubscriptionRepository, SubscriptionRepository>();
builder.Services.AddSingleton<IPaymentProviderClient, PaymentProviderClient>();
builder.Services.AddSingleton<SubscriptionHandler>();

builder.Services.AddControllers();
builder.AddApiDocumentation("Billing Service API", "Owns the SaaS self-service subscription and module purchase/entitlement per home.");

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dataSource = scope.ServiceProvider.GetRequiredService<NpgsqlDataSource>();
    await Schema.EnsureCreatedAsync(dataSource);
    app.Logger.LogInformation("Connected to database and verified schema");
}

app.UseApiDocumentation("Billing Service API");

app.MapGet("/health", () => Results.Ok(new { status = "ok", service = "billing" }));

app.MapControllers();

app.Logger.LogInformation("Billing Service starting on {Address}", $"http://0.0.0.0:{port}");
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
